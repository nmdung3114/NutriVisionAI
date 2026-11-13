namespace NutriVision.Application.Modules.Auth;

public record RegisterRequest(string HoTen, string Email, string MatKhau, string XacNhanMatKhau, bool DongYDieuKhoan);
public record RegisterResult(string Email, bool EmailDaGui);
public record VerifyEmailRequest(string Token);
public record VerifyEmailResult(bool DaXacThucTruocDo);
public record EmailRequest(string Email);
public record LoginRequest(string Email, string MatKhau);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string? RefreshToken);
public record ResetPasswordRequest(string Token, string MatKhauMoi, string XacNhanMatKhau);
public record ChangePasswordRequest(string MatKhauHienTai, string MatKhauMoi, string XacNhanMatKhau);

public record UserInfo(Guid Id, string Email, string HoTen, string VaiTro, string TrangThai, bool EmailDaXacThuc, string? AnhDaiDienUrl);
public record AccountInfo(Guid Id, string Email, string HoTen, string VaiTro, string TrangThai, bool EmailDaXacThuc, string? AnhDaiDienUrl, DateTimeOffset NgayTao);
public record AuthResult(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt, UserInfo User);
