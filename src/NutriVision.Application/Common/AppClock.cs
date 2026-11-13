using Microsoft.Extensions.Options;
using NutriVision.Application.Settings;

namespace NutriVision.Application.Common;

/// <summary>Đồng hồ ứng dụng: thời điểm UTC và "hôm nay" theo múi giờ cấu hình (mặc định Asia/Ho_Chi_Minh).</summary>
public class AppClock
{
    private readonly TimeProvider _time;
    private readonly TimeZoneInfo _zone;

    public AppClock(TimeProvider time, IOptions<AppSettings> settings)
    {
        _time = time;
        _zone = ResolveZone(settings.Value.TimeZone);
    }

    public DateTimeOffset UtcNow => _time.GetUtcNow();

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(UtcNow, _zone).DateTime);

    public DateOnly ToLocalDate(DateTimeOffset value) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(value, _zone).DateTime);

    /// <summary>Thời điểm UTC bắt đầu của một ngày (theo múi giờ cấu hình).</summary>
    public DateTimeOffset StartOfDay(DateOnly day)
    {
        var local = day.ToDateTime(TimeOnly.MinValue);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), _zone), TimeSpan.Zero);
    }

    private static TimeZoneInfo ResolveZone(string id)
    {
        foreach (var candidate in new[] { id, "Asia/Ho_Chi_Minh", "SE Asia Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(candidate); } catch (TimeZoneNotFoundException) { } catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.Utc;
    }
}
