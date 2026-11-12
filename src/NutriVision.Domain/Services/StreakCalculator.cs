namespace NutriVision.Domain.Services;

public readonly record struct StreakResult(int Current, int Longest, DateOnly? LongestEnd);

/// <summary>
/// Chuỗi ngày ghi nhận liên tiếp. Chuỗi hiện tại tính đến hôm nay; nếu hôm nay chưa ghi
/// thì vẫn giữ chuỗi kết thúc hôm qua (người dùng còn cả ngày để ghi tiếp, không bị "đứt" từ sáng).
/// </summary>
public static class StreakCalculator
{
    public static StreakResult Compute(IEnumerable<DateOnly> loggedDays, DateOnly today)
    {
        var days = loggedDays.Where(d => d <= today).Distinct().Order().ToList();
        if (days.Count == 0) return new StreakResult(0, 0, null);

        int longest = 1, run = 1;
        var longestEnd = days[0];
        for (var i = 1; i < days.Count; i++)
        {
            run = days[i].DayNumber - days[i - 1].DayNumber == 1 ? run + 1 : 1;
            if (run >= longest) { longest = run; longestEnd = days[i]; }
        }

        var set = days.ToHashSet();
        var cursor = set.Contains(today) ? today : today.AddDays(-1);
        var current = 0;
        while (set.Contains(cursor)) { current++; cursor = cursor.AddDays(-1); }
        return new StreakResult(current, longest, longestEnd);
    }
}
