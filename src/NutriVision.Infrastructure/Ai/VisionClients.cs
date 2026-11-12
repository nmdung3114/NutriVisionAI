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

/// <summary>Nhận diện món ăn bằng Google Gemini (đa phương thức, structured JSON output). Timeout + retry theo BR-20.</summary>
public class GeminiVisionClient(HttpClient http, IOptions<AiSettings> options, ILogger<GeminiVisionClient> logger) : IVisionAiClient
{
    private readonly AiSettings _s = options.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_s.ApiKey);

    private const string Prompt = """
        Bạn là chuyên gia dinh dưỡng Việt Nam. Hãy quan sát ảnh và nhận diện món ăn.
        Trả về JSON: isFood (có phải ảnh đồ ăn/thức uống không), tenMonAn (tên món bằng tiếng Việt, ngắn gọn, vd "Cơm tấm sườn bì chả"),
        doTinCay (0..1), thanhPhan: danh sách các thành phần/nguyên liệu nhìn thấy, mỗi phần có ten (tiếng Việt) và gram (ước lượng khối lượng thực tế đang có trong ảnh, đơn vị gram, > 0).
        Chỉ liệt kê thành phần thật sự nhìn thấy, tối đa 12. Nếu không phải đồ ăn, đặt isFood=false và thanhPhan rỗng.
        """;

    public async Task<VisionResult> AnalyzeAsync(byte[] image, string contentType, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return VisionResult.Fail("Dịch vụ nhận diện AI chưa được cấu hình trên máy chủ. Vui lòng nhập món ăn thủ công.");

        var sw = Stopwatch.StartNew();
        var attempts = Math.Max(1, _s.MaxRetries + 1);
        string lastError = "Dịch vụ AI không phản hồi. Vui lòng thử lại hoặc nhập thủ công.";

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(_s.TimeoutSeconds));
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post,
                    $"{_s.BaseUrl.TrimEnd('/')}/v1beta/models/{_s.Model}:generateContent")
                {
                    Content = JsonContent.Create(BuildBody(image, contentType)),
                };
                request.Headers.Add("x-goog-api-key", _s.ApiKey);

                using var response = await http.SendAsync(request, timeout.Token);
                var text = await response.Content.ReadAsStringAsync(timeout.Token);

                if (response.IsSuccessStatusCode)
                {
                    var parsed = Parse(text);
                    if (parsed is not null) return new VisionResult(true, parsed, null, (int)sw.ElapsedMilliseconds, text);
                    lastError = "AI trả về kết quả không hợp lệ.";
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500)
                {
                    lastError = "Dịch vụ AI đang quá tải. Vui lòng thử lại sau.";
                    logger.LogWarning("Gemini lỗi tạm thời {Status} (lần {Attempt}/{Max})", (int)response.StatusCode, attempt, attempts);
                }
                else
                {
                    // 4xx khác (sai key, sai cấu hình...) – thử lại vô ích.
                    logger.LogError("Gemini từ chối yêu cầu {Status}: {Body}", (int)response.StatusCode, Truncate(text));
                    return VisionResult.Fail("Dịch vụ AI từ chối yêu cầu (kiểm tra cấu hình khóa API). Vui lòng nhập thủ công.", (int)sw.ElapsedMilliseconds);
                }
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                lastError = $"Dịch vụ AI không phản hồi trong {_s.TimeoutSeconds} giây.";
                logger.LogWarning("Gemini timeout (lần {Attempt}/{Max})", attempt, attempts);
            }
            catch (HttpRequestException ex)
            {
                lastError = "Không kết nối được dịch vụ AI.";
                logger.LogWarning(ex, "Gemini lỗi mạng (lần {Attempt}/{Max})", attempt, attempts);
            }

            if (attempt < attempts) await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt), ct);
        }

        return VisionResult.Fail($"{lastError} Vui lòng nhập thủ công.", (int)sw.ElapsedMilliseconds);
    }

    private object BuildBody(byte[] image, string contentType) => new
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

    /// <summary>Đọc phản hồi Gemini → VisionDish. Public static để kiểm thử.</summary>
    public static VisionDish? Parse(string responseJson)
    {
        try
        {
            var root = JsonNode.Parse(responseJson);
            if (root?["candidates"] is not JsonArray { Count: > 0 } candidates) return null;
            if (candidates[0]?["content"]?["parts"] is not JsonArray { Count: > 0 } parts) return null;
            var text = parts[0]?["text"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(text)) return null;
            var json = JsonNode.Parse(text);
            if (json is null) return null;

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
