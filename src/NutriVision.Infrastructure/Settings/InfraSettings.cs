namespace NutriVision.Infrastructure.Settings;

public class JwtSettings
{
    public const string Section = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "NutriVision";
    public string Audience { get; set; } = "NutriVision.Web";
    public int AccessTokenMinutes { get; set; } = 30;
}

public class DatabaseSettings
{
    public const string Section = "Database";
    /// <summary>SqlServer (mặc định, theo tài liệu) hoặc Sqlite (chạy cục bộ không cần SQL Server).</summary>
    public string Provider { get; set; } = "SqlServer";
    public string ConnectionString { get; set; } = string.Empty;
    public bool AutoMigrate { get; set; } = true;
    public bool SeedFoods { get; set; } = true;
    public bool SeedDemoAccounts { get; set; } = false;
}

public class StorageSettings
{
    public const string Section = "Storage";
    public string RootPath { get; set; } = "App_Data/uploads";
    public string SigningKey { get; set; } = string.Empty;
    public int UrlLifetimeMinutes { get; set; } = 60;
}

public class EmailSettings
{
    public const string Section = "Email";
    /// <summary>Log = ghi email ra log + thư mục outbox (dev); Smtp = gửi thật.</summary>
    public string Provider { get; set; } = "Log";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseStartTls { get; set; } = true;
    public string FromAddress { get; set; } = "no-reply@nutrivision.ai";
    public string FromName { get; set; } = "NutriVision AI";
    public string OutboxPath { get; set; } = "App_Data/outbox";
}

public class AiSettings
{
    public const string Section = "Ai";
    /// <summary>Gemini | Http (dịch vụ AI riêng, xem docs/ai-integration.md) | None</summary>
    public string Provider { get; set; } = "None";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 2;
    /// <summary>Chỉ dùng cho Provider=Http: đường dẫn endpoint nhận diện.</summary>
    public string Path { get; set; } = "/analyze";
}
