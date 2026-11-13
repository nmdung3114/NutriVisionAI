using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NutriVision.Infrastructure.Ai;
using NutriVision.Infrastructure.Settings;
using Xunit;

namespace NutriVision.UnitTests;

/// <summary>Hợp đồng kết nối dịch vụ AI riêng của nhóm AI (docs/ai-integration.md).</summary>
public class HttpVisionClientTests
{
    private sealed class StubHandler(Func<int, HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public int Calls;
        public HttpRequestMessage? Last;
        public string? LastBody;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Last = request;
            LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return await respond(++Calls, request);
        }
    }

    private static (HttpVisionClient, StubHandler) Create(Func<int, HttpRequestMessage, Task<HttpResponseMessage>> respond, string apiKey = "secret", string baseUrl = "http://ai.local:8000")
    {
        var h = new StubHandler(respond);
        var s = Options.Create(new AiSettings { Provider = "Http", BaseUrl = baseUrl, ApiKey = apiKey, Path = "/analyze", TimeoutSeconds = 5, MaxRetries = 2 });
        return (new HttpVisionClient(new HttpClient(h), s, NullLogger<HttpVisionClient>.Instance), h);
    }

    private static Task<HttpResponseMessage> Json(HttpStatusCode code, object body) =>
        Task.FromResult(new HttpResponseMessage(code) { Content = new StringContent(body as string ?? JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") });

    private static readonly object Pho = new { isFood = true, tenMonAn = "Phở bò", doTinCay = 0.87, thanhPhan = new[] { new { ten = "Bánh phở", gram = 200 }, new { ten = "Thịt bò", gram = 80 } } };

    [Fact]
    public async Task Sends_contract_request_and_parses_result()
    {
        var (c, h) = Create((_, _) => Json(HttpStatusCode.OK, Pho));
        var r = await c.AnalyzeAsync([1, 2, 3], "image/jpeg");

        Assert.True(r.Success);
        Assert.Equal("Phở bò", r.Dish!.TenMonAn);
        Assert.Equal(0.87m, r.Dish.DoTinCay);
        Assert.Equal(2, r.Dish.ThanhPhan.Count);
        Assert.Equal("http://ai.local:8000/analyze", h.Last!.RequestUri!.ToString());
        Assert.Equal("Bearer secret", h.Last.Headers.Authorization!.ToString());
        using var body = JsonDocument.Parse(h.LastBody!);
        Assert.Equal("AQID", body.RootElement.GetProperty("image").GetString()); // base64 của [1,2,3]
        Assert.Equal("image/jpeg", body.RootElement.GetProperty("contentType").GetString());
        Assert.Equal("vi", body.RootElement.GetProperty("ngonNgu").GetString());
        Assert.NotNull(h.Last.Content!.Headers.ContentLength); // không gửi chunked
    }

    [Fact]
    public async Task No_auth_header_when_api_key_empty()
    {
        var (c, h) = Create((_, _) => Json(HttpStatusCode.OK, Pho), apiKey: "");
        await c.AnalyzeAsync([1], "image/png");
        Assert.Null(h.Last!.Headers.Authorization);
    }

    [Fact]
    public async Task Retries_on_503_then_succeeds_but_not_on_401()
    {
        var (c, h) = Create((n, _) => n < 3 ? Json(HttpStatusCode.ServiceUnavailable, "{}") : Json(HttpStatusCode.OK, Pho));
        Assert.True((await c.AnalyzeAsync([1], "image/png")).Success);
        Assert.Equal(3, h.Calls);

        var (c2, h2) = Create((_, _) => Json(HttpStatusCode.Unauthorized, "{\"error\":\"bad key\"}"));
        var r = await c2.AnalyzeAsync([1], "image/png");
        Assert.False(r.Success);
        Assert.Equal(1, h2.Calls);
    }

    [Fact]
    public async Task Invalid_body_fails_and_not_food_returns_empty_dish()
    {
        var (bad, _) = Create((_, _) => Json(HttpStatusCode.OK, "<html>oops</html>"));
        Assert.False((await bad.AnalyzeAsync([1], "image/png")).Success);

        var (cat, _) = Create((_, _) => Json(HttpStatusCode.OK, new { isFood = false, tenMonAn = "Con mèo", thanhPhan = new[] { new { ten = "mèo", gram = 3000 } } }));
        var r = await cat.AnalyzeAsync([1], "image/png");
        Assert.True(r.Success);
        Assert.Equal("", r.Dish!.TenMonAn);
        Assert.Empty(r.Dish.ThanhPhan);
    }

    [Fact]
    public void Not_configured_without_valid_base_url()
    {
        var (c, _) = Create((_, _) => Json(HttpStatusCode.OK, Pho), baseUrl: "");
        Assert.False(c.IsConfigured);
    }
}
