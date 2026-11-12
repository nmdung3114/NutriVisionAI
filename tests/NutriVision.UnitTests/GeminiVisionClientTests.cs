using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NutriVision.Infrastructure.Ai;
using NutriVision.Infrastructure.Settings;
using Xunit;

namespace NutriVision.UnitTests;

public class GeminiVisionClientTests
{
    private static string GeminiResponse(object payload) => JsonSerializer.Serialize(new
    {
        candidates = new[] { new { content = new { parts = new[] { new { text = JsonSerializer.Serialize(payload) } } } } },
    });

    private sealed class StubHandler(Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public int Calls;
        public HttpRequestMessage? Last;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Last = request;
            return respond(++Calls, request, ct);
        }
    }

    private static (GeminiVisionClient Client, StubHandler Handler) Create(Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond, int timeoutSeconds = 30)
    {
        var h = new StubHandler(respond);
        var s = Options.Create(new AiSettings { Provider = "Gemini", ApiKey = "k", Model = "gemini-2.5-flash", TimeoutSeconds = timeoutSeconds, MaxRetries = 2 });
        return (new GeminiVisionClient(new HttpClient(h), s, NullLogger<GeminiVisionClient>.Instance), h);
    }

    private static HttpResponseMessage Json(HttpStatusCode code, string body) => new(code) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    [Fact]
    public void Parse_Reads_Structured_Output()
    {
        var dish = GeminiVisionClient.Parse(GeminiResponse(new
        {
            isFood = true, tenMonAn = "Phở bò", doTinCay = 0.91,
            thanhPhan = new object[] { new { ten = "Bánh phở", gram = 200 }, new { ten = "Thịt bò", gram = 80.5 }, new { ten = "", gram = 5 }, new { ten = "Hành", gram = 0 } },
        }));
        Assert.NotNull(dish);
        Assert.Equal("Phở bò", dish!.TenMonAn);
        Assert.Equal(0.91m, dish.DoTinCay);
        Assert.Equal(2, dish.ThanhPhan.Count); // bỏ thành phần rỗng / gram ≤ 0
    }

    [Fact]
    public void Parse_NotFood_Returns_Empty_And_Garbage_Returns_Null()
    {
        var d = GeminiVisionClient.Parse(GeminiResponse(new { isFood = false, tenMonAn = "Con mèo", thanhPhan = new[] { new { ten = "mèo", gram = 3000 } } }));
        Assert.Empty(d!.ThanhPhan);
        Assert.Null(GeminiVisionClient.Parse("{\"candidates\":[]}"));
        Assert.Null(GeminiVisionClient.Parse("not json"));
    }

    [Fact] // BR-20: lỗi tạm thời → thử lại tối đa 2 lần
    public async Task Retries_On_Transient_Error_Then_Succeeds()
    {
        var (client, h) = Create((n, _, _) => Task.FromResult(n < 3
            ? Json(HttpStatusCode.ServiceUnavailable, "{}")
            : Json(HttpStatusCode.OK, GeminiResponse(new { isFood = true, tenMonAn = "Cơm", doTinCay = 0.8, thanhPhan = new[] { new { ten = "Cơm", gram = 150 } } }))));
        var r = await client.AnalyzeAsync([1, 2, 3], "image/png");
        Assert.True(r.Success);
        Assert.Equal(3, h.Calls);
        Assert.Equal("k", h.Last!.Headers.GetValues("x-goog-api-key").Single());
        Assert.Contains("gemini-2.5-flash:generateContent", h.Last.RequestUri!.ToString());
    }

    [Fact] // AC-26: hết 3 lần (timeout) → thất bại để chuyển nhập tay
    public async Task Gives_Up_After_Timeouts()
    {
        var (client, h) = Create(async (_, _, ct) => { await Task.Delay(TimeSpan.FromSeconds(10), ct); return Json(HttpStatusCode.OK, "{}"); }, timeoutSeconds: 1);
        var r = await client.AnalyzeAsync([1], "image/png");
        Assert.False(r.Success);
        Assert.Equal(3, h.Calls);
        Assert.Contains("nhập thủ công", r.Error);
    }

    [Fact]
    public async Task Does_Not_Retry_On_Client_Error()
    {
        var (client, h) = Create((_, _, _) => Task.FromResult(Json(HttpStatusCode.Forbidden, "{\"error\":\"bad key\"}")));
        var r = await client.AnalyzeAsync([1], "image/png");
        Assert.False(r.Success);
        Assert.Equal(1, h.Calls);
    }
}
