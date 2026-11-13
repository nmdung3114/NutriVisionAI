using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NutriVision.Application.Abstractions;

namespace NutriVision.IntegrationTests;

public sealed class FakeVisionClient : IVisionAiClient
{
    public bool IsConfigured => true;
    public Func<VisionResult> Next { get; set; } = Default;
    public int Calls;

    public static VisionResult Default() => new(true, new VisionDish("Cơm tấm sườn", 0.92m,
        [new VisionIngredient("Cơm trắng", 200), new VisionIngredient("Sườn heo nướng mật ong", 100), new VisionIngredient("Món lạ hoắc", 30)]), null, 850, "{}");

    public Task<VisionResult> AnalyzeAsync(byte[] image, string contentType, CancellationToken ct = default)
    {
        Interlocked.Increment(ref Calls);
        return Task.FromResult(Next());
    }
}

public sealed class CapturingEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Html)> Sent { get; } = [];
    public Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        lock (Sent) Sent.Add((toEmail, subject, htmlBody));
        return Task.CompletedTask;
    }

    /// <summary>Lấy token từ link trong email gần nhất gửi tới địa chỉ.</summary>
    public string TokenFor(string email)
    {
        string html;
        lock (Sent) html = Sent.Last(s => s.To == email).Html;
        var i = html.IndexOf("token=", StringComparison.Ordinal) + 6;
        var j = html.IndexOfAny(['"', '&'], i);
        return Uri.UnescapeDataString(html[i..j]);
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public FakeVisionClient Vision { get; } = new();
    public CapturingEmailSender Email { get; } = new();
    private static readonly string Dir = Path.Combine(Path.GetTempPath(), "nv-tests-" + Guid.NewGuid().ToString("N"));
    private static bool UsesSqlServer;

    public override async ValueTask DisposeAsync()
    {
        if (UsesSqlServer)
        {
            using var scope = Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<NutriVision.Infrastructure.Persistence.ApplicationDbContext>().Database.EnsureDeletedAsync();
        }
        await base.DisposeAsync();
    }

    // Program.cs đọc cấu hình ngay khi tạo builder, nên dùng biến môi trường (có hiệu lực từ đầu).
    static ApiFactory()
    {
        Directory.CreateDirectory(Dir);
        void Set(string k, string v) => Environment.SetEnvironmentVariable(k, v);
        // Mặc định SQLite tạm. Đặt NV_TEST_SQLSERVER (vd "Server=localhost,1433;User Id=sa;Password=...;TrustServerCertificate=True")
        // để chạy toàn bộ integration test trên SQL Server thật: tạo database riêng nv_test_<guid>, xóa khi xong.
        var sqlServer = Environment.GetEnvironmentVariable("NV_TEST_SQLSERVER");
        if (string.IsNullOrWhiteSpace(sqlServer))
        {
            Set("Database__Provider", "Sqlite");
            Set("Database__ConnectionString", $"Data Source={Path.Combine(Dir, "test.db")}");
        }
        else
        {
            Set("Database__Provider", "SqlServer");
            Set("Database__ConnectionString", $"{sqlServer.TrimEnd(';')};Database=nv_test_{Guid.NewGuid():N}");
            UsesSqlServer = true;
        }
        Set("Database__SeedDemoAccounts", "true");
        Set("Jwt__Secret", "integration-test-secret-key-0123456789-abcdef");
        Set("Storage__RootPath", Path.Combine(Dir, "uploads"));
        Set("Email__OutboxPath", Path.Combine(Dir, "outbox"));
        Set("RateLimit__AuthPerMinute", "100000");
        Set("Serilog__MinimumLevel__Default", "Warning");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(s =>
        {
            s.RemoveAll<IVisionAiClient>();
            s.AddSingleton<IVisionAiClient>(Vision);
            s.RemoveAll<IEmailSender>();
            s.AddSingleton<IEmailSender>(Email);
        });
    }

}

public sealed record Resp(int Status, JsonElement Json, byte[] Raw, HttpResponseMessage Message)
{
    public bool Success => Json.ValueKind == JsonValueKind.Object && Json.GetProperty("success").GetBoolean();
    public string? Code => Json.ValueKind == JsonValueKind.Object && Json.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.String ? c.GetString() : null;
    public JsonElement Data => Json.GetProperty("data");
}

public class ApiClient(HttpClient http)
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }

    public async Task<Resp> Send(HttpMethod method, string url, object? body = null, HttpContent? content = null, bool auth = true)
    {
        using var req = new HttpRequestMessage(method, url);
        if (content is not null) req.Content = content;
        else if (body is not null) req.Content = JsonContent.Create(body);
        if (auth && AccessToken is not null) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        var res = await http.SendAsync(req);
        var raw = await res.Content.ReadAsByteArrayAsync();
        JsonElement json = default;
        if (res.Content.Headers.ContentType?.MediaType == "application/json" && raw.Length > 0)
            json = JsonDocument.Parse(raw).RootElement.Clone();
        return new Resp((int)res.StatusCode, json, raw, res);
    }

    public Task<Resp> Get(string url) => Send(HttpMethod.Get, url);
    public Task<Resp> Post(string url, object? body = null) => Send(HttpMethod.Post, url, body ?? new { });
    public Task<Resp> Put(string url, object body) => Send(HttpMethod.Put, url, body);
    public Task<Resp> Delete(string url) => Send(HttpMethod.Delete, url);

    public async Task<Resp> Login(string email, string password)
    {
        var r = await Send(HttpMethod.Post, "/api/auth/login", new { email, matKhau = password }, auth: false);
        if (r.Success) { AccessToken = r.Data.GetProperty("accessToken").GetString(); RefreshToken = r.Data.GetProperty("refreshToken").GetString(); }
        return r;
    }

    public Task<Resp> Scan(byte[] image, string fileName, string loai = "Trua", string contentType = "image/png")
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(image);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "image", fileName);
        form.Add(new StringContent(loai), "loaiBuaAn");
        return Send(HttpMethod.Post, "/api/meals/scan", content: form);
    }

    /// <summary>Tạo ảnh PNG hợp lệ (RGB một màu) theo đặc tả PNG: IHDR + IDAT (zlib) + IEND, có CRC.</summary>
    public static byte[] Png(int w = 320, int h = 320)
    {
        var raw = new byte[h * (1 + w * 3)];
        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var i = y * (1 + w * 3) + 1 + x * 3; // byte đầu mỗi dòng = filter 0
                raw[i] = 200; raw[i + 1] = 120; raw[i + 2] = 60;
            }
        using var z = new MemoryStream();
        using (var zs = new System.IO.Compression.ZLibStream(z, System.IO.Compression.CompressionLevel.Fastest, leaveOpen: true))
            zs.Write(raw);

        var ihdr = new byte[13];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(ihdr, w);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4), h);
        ihdr[8] = 8; ihdr[9] = 2; // 8 bit, truecolor

        using var ms = new MemoryStream();
        ms.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        PngChunk(ms, "IHDR", ihdr);
        PngChunk(ms, "IDAT", z.ToArray());
        PngChunk(ms, "IEND", []);
        return ms.ToArray();
    }

    private static void PngChunk(Stream s, string type, byte[] data)
    {
        Span<byte> buf = stackalloc byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(buf, data.Length);
        s.Write(buf);
        var typeAndData = System.Text.Encoding.ASCII.GetBytes(type).Concat(data).ToArray();
        s.Write(typeAndData);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(buf, Crc32(typeAndData));
        s.Write(buf);
    }

    private static uint Crc32(byte[] data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
        {
            crc ^= b;
            for (var k = 0; k < 8; k++) crc = (crc & 1) != 0 ? 0xEDB88320u ^ (crc >> 1) : crc >> 1;
        }
        return ~crc;
    }
}

[CollectionDefinition("api")]
public class ApiCollection : ICollectionFixture<ApiFactory>;

/// <summary>Một host + một CSDL dùng chung cho toàn bộ test (mỗi test tự tạo tài khoản riêng).</summary>
[Collection("api")]
public abstract class IntegrationTestBase(ApiFactory factory)
{
    protected ApiFactory Factory => factory;
    protected ApiClient NewClient() => new(factory.CreateClient());

    protected Task<ApiClient> Anonymous() => Task.FromResult(NewClient());

    protected async Task<ApiClient> LoginAs(string email, string pw)
    {
        var c = NewClient();
        var r = await c.Login(email, pw);
        Assert.True(r.Success, $"login {email} failed: {r.Code}");
        return c;
    }

    protected Task<ApiClient> LoginUser() => LoginAs("user@nutrivision.ai", "User@123");
    protected Task<ApiClient> LoginAdmin() => LoginAs("admin@nutrivision.ai", "Admin@123");

    /// <summary>Tạo tài khoản mới đã xác thực + hồ sơ, trả về client đã đăng nhập.</summary>
    protected async Task<(ApiClient Client, string Email)> NewVerifiedUser(string password = "Passw0rd1", bool withProfile = true)
    {
        var email = $"u{Guid.NewGuid():N}@test.vn";
        var c = NewClient();
        var reg = await c.Post("/api/auth/register", new { hoTen = "Người Test", email, matKhau = password, xacNhanMatKhau = password, dongYDieuKhoan = true });
        Assert.True(reg.Success, reg.Code);
        var ver = await c.Post("/api/auth/verify-email", new { token = factory.Email.TokenFor(email) });
        Assert.True(ver.Success, ver.Code);
        Assert.True((await c.Login(email, password)).Success);
        if (withProfile)
        {
            var p = await c.Put("/api/profile", new { hoTen = "Người Test", ngaySinh = "1998-05-20", gioiTinh = "Nu", chieuCao = 160, canNang = 55, mucDoVanDong = "VuaPhai", mucTieuCalo = 2000 });
            Assert.True(p.Success, p.Code);
        }
        return (c, email);
    }

    protected static object Comp(string name, decimal amount, string unit = "g", Guid? foodId = null, object? manual = null) =>
        new { tenThanhPhan = name, thucPhamThamChieuId = foodId, khauPhan = amount, donViTinh = unit, nhapTay = manual };
}
