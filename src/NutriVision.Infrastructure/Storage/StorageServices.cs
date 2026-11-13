using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Infrastructure.Settings;

namespace NutriVision.Infrastructure.Storage;

/// <summary>Lưu file lên đĩa cục bộ; có thể thay bằng S3/MinIO nhờ interface IFileStorage.</summary>
public class LocalFileStorage(IOptions<StorageSettings> options) : IFileStorage
{
    private readonly string _root = Path.GetFullPath(options.Value.RootPath);

    public async Task<StoredFile> SaveAsync(string folder, Stream content, string extension, CancellationToken ct = default)
    {
        var rel = $"{folder.Trim('/')}/{Guid.NewGuid():N}{extension}";
        var full = ResolveSafe(rel);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await using var fs = File.Create(full);
        await content.CopyToAsync(fs, ct);
        return new StoredFile(rel, fs.Length);
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken ct = default)
    {
        var full = ResolveSafe(relativePath);
        Stream? s = File.Exists(full) ? new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
        return Task.FromResult(s);
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        try
        {
            var full = ResolveSafe(relativePath);
            if (File.Exists(full)) File.Delete(full);
        }
        catch (IOException) { /* best-effort */ }
        return Task.CompletedTask;
    }

    /// <summary>Chặn path traversal: đường dẫn phải nằm trong thư mục gốc.</summary>
    private string ResolveSafe(string relativePath)
    {
        var full = Path.GetFullPath(Path.Combine(_root, relativePath));
        if (!full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Đường dẫn file không hợp lệ.");
        return full;
    }
}

/// <summary>URL ảnh ký HMAC-SHA256 có thời hạn: &lt;img&gt; không gửi được Bearer nên dùng chữ ký thay thế.</summary>
public class HmacFileUrlSigner(IOptions<StorageSettings> options, AppClock clock) : IFileUrlSigner
{
    private readonly StorageSettings _s = options.Value;

    public string? Sign(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;
        var exp = clock.UtcNow.AddMinutes(_s.UrlLifetimeMinutes).ToUnixTimeSeconds();
        return $"/api/files/{relativePath}?exp={exp}&sig={Compute(relativePath, exp)}";
    }

    public bool Verify(string relativePath, long expiresUnix, string signature)
    {
        if (expiresUnix < clock.UtcNow.ToUnixTimeSeconds()) return false;
        var expected = Encoding.ASCII.GetBytes(Compute(relativePath, expiresUnix));
        var actual = Encoding.ASCII.GetBytes(signature ?? string.Empty);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    private string Compute(string path, long exp)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(_s.SigningKey));
        return Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes($"{path}|{exp}"))).ToLowerInvariant();
    }
}
