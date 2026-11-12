using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NutriVision.Application.Modules.Meals;

namespace NutriVision.Infrastructure.Jobs;

/// <summary>Định kỳ dọn bản nháp bỏ dở và ảnh đã upload (BR-25).</summary>
public class DraftCleanupService(IServiceScopeFactory scopes, ILogger<DraftCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var n = await scope.ServiceProvider.GetRequiredService<IMealService>().CleanupAbandonedDraftsAsync(stoppingToken);
                if (n > 0) logger.LogInformation("Đã dọn {Count} bản nháp bữa ăn bỏ dở", n);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { logger.LogError(ex, "Dọn bản nháp thất bại"); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
