using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NutriVision.Application.Abstractions;
using NutriVision.Infrastructure.Settings;

namespace NutriVision.Infrastructure.Ai;

/// <summary>Dùng khi chưa cấu hình AI: luôn báo không khả dụng để UI chuyển sang nhập tay (NFR-14).</summary>
public class NullVisionClient : IVisionAiClient
{
    public bool IsConfigured => false;

    public Task<VisionResult> AnalyzeAsync(byte[] image, string contentType, CancellationToken ct = default) =>
        Task.FromResult(VisionResult.Fail("Dịch vụ nhận diện AI chưa được cấu hình trên máy chủ. Vui lòng nhập món ăn thủ công."));
}

/// <summary>
/// Khung chung cho client AI qua HTTP: timeout mỗi lần gọi, thử lại khi lỗi tạm thời (429/5xx/mạng/timeout),
/// không thử lại khi 4xx khác (sai key/cấu hình). Lớp con chỉ cần dựng request và đọc kết quả.
/// </summary>
public abstract class RetryingVisionClient(HttpClient http, AiSettings s, ILogger logger, string name) : IVisionAiClient
{
    protected AiSettings Settings => s;
    public abstract bool IsConfigured { get; }
    protected abstract HttpRequestMessage CreateRequest(byte[] image, string contentType);
    protected abstract VisionDish? ParseResponse(string body);

    public async Task<VisionResult> AnalyzeAsync(byte[] image, string contentType, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return VisionResult.Fail("Dịch vụ nhận diện AI chưa được cấu hình trên máy chủ. Vui lòng nhập món ăn thủ công.");

        var sw = Stopwatch.StartNew();
        var attempts = Math.Max(1, s.MaxRetries + 1);
        string lastError = "Dịch vụ AI không phản hồi. Vui lòng thử lại hoặc nhập thủ công.";

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(s.TimeoutSeconds));
            try
            {
                using var request = CreateRequest(image, contentType);
                using var response = await http.SendAsync(request, timeout.Token);
                var text = await response.Content.ReadAsStringAsync(timeout.Token);

                if (response.IsSuccessStatusCode)
                {
                    var parsed = ParseResponse(text);
                    if (parsed is not null) return new VisionResult(true, parsed, null, (int)sw.ElapsedMilliseconds, text);
                    lastError = "AI trả về kết quả không hợp lệ.";
                    logger.LogWarning("{Ai} trả về nội dung không đúng định dạng: {Body}", name, Truncate(text));
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500)
                {
                    lastError = "Dịch vụ AI đang quá tải. Vui lòng thử lại sau.";
                    logger.LogWarning("{Ai} lỗi tạm thời {Status} (lần {Attempt}/{Max})", name, (int)response.StatusCode, attempt, attempts);
                }
                else
                {
                    // 4xx khác (sai key, sai cấu hình...) – thử lại vô ích.
                    logger.LogError("{Ai} từ chối yêu cầu {Status}: {Body}", name, (int)response.StatusCode, Truncate(text));
                    return VisionResult.Fail("Dịch vụ AI từ chối yêu cầu (kiểm tra cấu hình khóa API). Vui lòng nhập thủ công.", (int)sw.ElapsedMilliseconds);
                }
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                lastError = $"Dịch vụ AI không phản hồi trong {s.TimeoutSeconds} giây.";
                logger.LogWarning("{Ai} timeout (lần {Attempt}/{Max})", name, attempt, attempts);
            }
            catch (HttpRequestException ex)
            {
                lastError = "Không kết nối được dịch vụ AI.";
                logger.LogWarning(ex, "{Ai} lỗi mạng (lần {Attempt}/{Max})", name, attempt, attempts);
            }

            if (attempt < attempts) await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt), ct);
        }

        return VisionResult.Fail($"{lastError} Vui lòng nhập thủ công.", (int)sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Đọc object kết quả chuẩn {isFood, tenMonAn, doTinCay, thanhPhan:[{ten, gram}]} (dùng chung cho Gemini và HTTP).
    /// Bỏ thành phần rỗng hoặc gram ≤ 0; ảnh không phải đồ ăn → tên rỗng, không thành phần.
    /// </summary>
    public static VisionDish? ParseDish(JsonNode? json)
    {
        if (json is not JsonObject) return null;
        try
        {
            var isFood = json["isFood"]?.GetValue<bool>() ?? true;
            var name = json["tenMonAn"]?.GetValue<string>()?.Trim() ?? string.Empty;
            decimal? conf = json["doTinCay"] is JsonNode c ? (decimal?)c.GetValue<double>() : null;
            var items = new List<VisionIngredient>();
            if (isFood && json["thanhPhan"] is JsonArray arr)
            {
                foreach (var n in arr)
                {
                    var ten = n?["ten"]?.GetValue<string>()?.Trim();
                    var gram = n?["gram"] is JsonNode g ? (decimal)g.GetValue<double>() : 0m;
                    if (!string.IsNullOrWhiteSpace(ten) && gram > 0) items.Add(new VisionIngredient(ten, Math.Round(gram, 1)));
                }
            }
            return new VisionDish(isFood ? name : string.Empty, conf, items);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException or ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string Truncate(string s) => s.Length <= 300 ? s : s[..300];
}

/// <summary>Nhận diện món ăn bằng Google Gemini (đa phương thức, structured JSON output). Timeout + retry theo BR-20.</summary>
public class GeminiVisionClient(HttpClient http, IOptions<AiSettings> options, ILogger<GeminiVisionClient> logger)
    : RetryingVisionClient(http, options.Value, logger, "Gemini")
{
    public override bool IsConfigured => !string.IsNullOrWhiteSpace(Settings.ApiKey);

    private const string Prompt = """
        Bạn là chuyên gia dinh dưỡng Việt Nam. Hãy quan sát ảnh và nhận diện món ăn.
        Trả về JSON: isFood (có phải ảnh đồ ăn/thức uống không), tenMonAn (tên món bằng tiếng Việt, ngắn gọn, vd "Cơm tấm sườn bì chả"),
        doTinCay (0..1), thanhPhan: danh sách các thành phần/nguyên liệu nhìn thấy, mỗi phần có ten (tiếng Việt) và gram (ước lượng khối lượng thực tế đang có trong ảnh, đơn vị gram, > 0).
        Chỉ liệt kê thành phần thật sự nhìn thấy, tối đa 12. Nếu không phải đồ ăn, đặt isFood=false và thanhPhan rỗng.
        """;

    protected override HttpRequestMessage CreateRequest(byte[] image, string contentType)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Settings.BaseUrl.TrimEnd('/')}/v1beta/models/{Settings.Model}:generateContent")
        {
            Content = JsonContent.Create(BuildBody(image, contentType)),
        };
        request.Headers.Add("x-goog-api-key", Settings.ApiKey);
        return request;
    }

    protected override VisionDish? ParseResponse(string body) => Parse(body);

    private static object BuildBody(byte[] image, string contentType) => new
    {
        contents = new[]
        {
            new { parts = new object[] { new { text = Prompt }, new { inline_data = new { mime_type = contentType, data = Convert.ToBase64String(image) } } } },
        },
        generationConfig = new
        {
            temperature = 0.2,
            responseMimeType = "application/json",
            responseSchema = new
            {
                type = "OBJECT",
                properties = new
                {
                    isFood = new { type = "BOOLEAN" },
                    tenMonAn = new { type = "STRING" },
                    doTinCay = new { type = "NUMBER" },
                    thanhPhan = new
                    {
                        type = "ARRAY",
                        items = new { type = "OBJECT", properties = new { ten = new { type = "STRING" }, gram = new { type = "NUMBER" } }, required = new[] { "ten", "gram" } },
                    },
                },
                required = new[] { "isFood", "tenMonAn", "thanhPhan" },
            },
        },
    };

    /// <summary>Đọc phản hồi Gemini (candidates[0].content.parts[0].text là JSON) → VisionDish. Public static để kiểm thử.</summary>
    public static VisionDish? Parse(string responseJson)
    {
        try
        {
            var root = JsonNode.Parse(responseJson);
            if (root?["candidates"] is not JsonArray { Count: > 0 } candidates) return null;
            if (candidates[0]?["content"]?["parts"] is not JsonArray { Count: > 0 } parts) return null;
            var text = parts[0]?["text"]?.GetValue<string>();
            return string.IsNullOrWhiteSpace(text) ? null : ParseDish(JsonNode.Parse(text));
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            return null;
        }
    }
}

/// <summary>
/// Kết nối dịch vụ AI riêng của nhóm (Python/FastAPI, …) theo hợp đồng docs/ai-integration.md:
/// POST {BaseUrl}{Path} với JSON {image: base64, contentType, ngonNgu: "vi"}, header Authorization: Bearer {ApiKey} nếu có;
/// trả về 200 với JSON {isFood, tenMonAn, doTinCay, thanhPhan:[{ten, gram}]}.
/// </summary>
public class HttpVisionClient(HttpClient http, IOptions<AiSettings> options, ILogger<HttpVisionClient> logger)
    : RetryingVisionClient(http, options.Value, logger, "AI HTTP")
{
    public override bool IsConfigured => Uri.TryCreate(Settings.BaseUrl, UriKind.Absolute, out _);

    protected override HttpRequestMessage CreateRequest(byte[] image, string contentType)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Settings.BaseUrl.TrimEnd('/')}/{Settings.Path.TrimStart('/')}")
        {
            // Body có Content-Length cố định (không chunked) để mọi framework phía AI đọc được.
            Content = new StringContent(JsonSerializer.Serialize(new { image = Convert.ToBase64String(image), contentType, ngonNgu = "vi" }), System.Text.Encoding.UTF8, "application/json"),
        };
        if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Settings.ApiKey);
        return request;
    }

    protected override VisionDish? ParseResponse(string body)
    {
        try { return ParseDish(JsonNode.Parse(body)); }
        catch (JsonException) { return null; }
    }
}
