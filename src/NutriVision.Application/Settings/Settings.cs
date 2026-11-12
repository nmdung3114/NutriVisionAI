namespace NutriVision.Application.Settings;

public class AppSettings
{
    public const string Section = "App";
    /// <summary>URL frontend, dùng tạo link trong email xác thực / đặt lại mật khẩu.</summary>
    public string FrontendUrl { get; set; } = "http://localhost:3000";
    public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    /// <summary>Số giờ sau đó bản nháp chưa xác nhận bị dọn (xóa cả ảnh).</summary>
    public int DraftRetentionHours { get; set; } = 24;
}

/// <summary>Chính sách xác thực (FR-02, FR-03, FR-05, BR-04..BR-07).</summary>
public class AuthPolicySettings
{
    public const string Section = "AuthPolicy";
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public int EmailVerifyHours { get; set; } = 24;
    public int PasswordResetMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
    public int ResendCooldownSeconds { get; set; } = 60;
}

public class UploadLimits
{
    public const long MealImageMaxBytes = 10L * 1024 * 1024;
    public const long AvatarMaxBytes = 5L * 1024 * 1024;
    public const int MealImageMinSide = 224;
}
