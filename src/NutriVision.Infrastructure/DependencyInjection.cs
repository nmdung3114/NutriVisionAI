using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Modules.Reports;
using NutriVision.Infrastructure.Ai;
using NutriVision.Infrastructure.Email;
using NutriVision.Infrastructure.Jobs;
using NutriVision.Infrastructure.Persistence;
using NutriVision.Infrastructure.Reports;
using NutriVision.Infrastructure.Security;
using NutriVision.Infrastructure.Settings;
using NutriVision.Infrastructure.Storage;

namespace NutriVision.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection(JwtSettings.Section));
        services.Configure<DatabaseSettings>(config.GetSection(DatabaseSettings.Section));
        services.Configure<StorageSettings>(config.GetSection(StorageSettings.Section));
        services.Configure<EmailSettings>(config.GetSection(EmailSettings.Section));
        services.Configure<AiSettings>(config.GetSection(AiSettings.Section));

        var db = config.GetSection(DatabaseSettings.Section).Get<DatabaseSettings>() ?? new DatabaseSettings();
        services.AddDbContext<ApplicationDbContext>(o =>
        {
            if (db.Provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
                o.UseSqlite(db.ConnectionString);
            else
                o.UseSqlServer(db.ConnectionString, sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        var email = config.GetSection(EmailSettings.Section).Get<EmailSettings>() ?? new EmailSettings();
        if (email.Provider.Equals("Smtp", StringComparison.OrdinalIgnoreCase)) services.AddSingleton<IEmailSender, SmtpEmailSender>();
        else services.AddSingleton<IEmailSender, OutboxEmailSender>();

        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IFileUrlSigner, HmacFileUrlSigner>();
        services.AddSingleton<IImageInspector, ImageHeaderInspector>();
        services.AddSingleton<IReportExporter, ReportExporter>();

        var ai = config.GetSection(AiSettings.Section).Get<AiSettings>() ?? new AiSettings();
        if (ai.Provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(ai.ApiKey))
            services.AddHttpClient<IVisionAiClient, GeminiVisionClient>(c => c.Timeout = Timeout.InfiniteTimeSpan); // timeout do client tự quản lý
        else
            services.AddSingleton<IVisionAiClient, NullVisionClient>();

        services.AddHostedService<DraftCleanupService>();
        return services;
    }
}
