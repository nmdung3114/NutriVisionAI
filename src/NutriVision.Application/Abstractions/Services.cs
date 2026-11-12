using NutriVision.Domain.Entities;

namespace NutriVision.Application.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Role { get; }
    string? IpAddress { get; }
    bool IsAuthenticated { get; }
    Guid RequireUserId();
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(NguoiDung user);
    /// <summary>Sinh chuỗi ngẫu nhiên an toàn (opaque token) dùng cho refresh / xác thực email / đặt lại mật khẩu.</summary>
    string GenerateOpaqueToken();
    /// <summary>Băm token (SHA-256 hex) trước khi lưu DB.</summary>
    string HashToken(string token);
}

public interface IEmailSender
{
    /// <summary>Gửi email HTML. Ném exception khi thất bại.</summary>
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default);
}

public sealed record StoredFile(string RelativePath, long Size);

public interface IFileStorage
{
    Task<StoredFile> SaveAsync(string folder, Stream content, string extension, CancellationToken ct = default);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken ct = default);
    Task DeleteAsync(string relativePath, CancellationToken ct = default);
}

public interface IFileUrlSigner
{
    /// <summary>Trả về URL tương đối đã ký, có hạn, hoặc null nếu path rỗng.</summary>
    string? Sign(string? relativePath);
    bool Verify(string relativePath, long expiresUnix, string signature);
}

public sealed record ImageInfo(int Width, int Height, string Format, string Extension, string ContentType);

public interface IImageInspector
{
    /// <summary>Đọc header ảnh; trả null nếu không phải ảnh jpg/png/webp hợp lệ.</summary>
    Task<ImageInfo?> InspectAsync(Stream content, CancellationToken ct = default);
}

public sealed record VisionIngredient(string Ten, decimal Gram);
public sealed record VisionDish(string TenMonAn, decimal? DoTinCay, IReadOnlyList<VisionIngredient> ThanhPhan);
public sealed record VisionResult(bool Success, VisionDish? Dish, string? Error, int ProcessingMs, string? RawJson)
{
    public static VisionResult Fail(string error, int ms = 0, string? raw = null) => new(false, null, error, ms, raw);
}

/// <summary>Dịch vụ AI đa phương thức nhận diện món ăn (FR-17). Hiện thực chịu trách nhiệm timeout/retry (BR-20).</summary>
public interface IVisionAiClient
{
    bool IsConfigured { get; }
    Task<VisionResult> AnalyzeAsync(byte[] image, string contentType, CancellationToken ct = default);
}
