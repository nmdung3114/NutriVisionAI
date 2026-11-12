using NutriVision.Application.Modules.Habits;
using NutriVision.Domain.ValueObjects;
using Xunit;

namespace NutriVision.UnitTests;

public class WeeklyInsightRulesTests
{
    private static readonly DateOnly D0 = new(2026, 11, 6);

    private static InsightDay Day(int i, decimal calo, decimal dam, decimal bot, decimal beo, decimal xo, decimal sang = 0, decimal toi = 0, decimal target = 2000) =>
        new(D0.AddDays(i), new NutritionValues(calo, dam, bot, beo, xo), target,
            new Dictionary<string, decimal> { ["Sang"] = sang, ["Trua"] = calo - sang - toi, ["Toi"] = toi });

    private static IReadOnlyList<InsightDay> Week(Func<int, InsightDay> f, int n = 7) => Enumerable.Range(0, n).Select(f).ToList();
    private static HashSet<string> Codes(IReadOnlyList<InsightDto> r) => r.Select(i => i.Ma).ToHashSet();

    [Fact]
    public void No_data_gives_single_info_message()
    {
        var r = WeeklyInsightRules.Build([], []);
        Assert.Equal("NO_DATA", Assert.Single(r).Ma);
    }

    [Fact]
    public void Balanced_week_is_praised()
    {
        // 2000 kcal: 100 g đạm, 250 g bột, 67 g béo (≈30%), 30 g xơ, có bữa sáng, tối 30%
        var r = WeeklyInsightRules.Build(Week(i => Day(i, 2000, 100, 250, 67, 30, sang: 500, toi: 600)), []);
        Assert.Equal(new HashSet<string> { "CALO_OK", "FIBER_OK" }, Codes(r));
        Assert.All(r, i => Assert.Equal(WeeklyInsightRules.TichCuc, i.Loai));
    }

    [Fact]
    public void Unhealthy_week_raises_warnings_first()
    {
        // 2600 kcal (130%), ít đạm, nhiều béo (~45%), ít xơ, bỏ bữa sáng, bữa tối 55%
        var r = WeeklyInsightRules.Build(Week(i => Day(i, 2600, 50, 260, 130, 8, sang: 0, toi: 1430)), []);
        var codes = Codes(r);
        foreach (var c in new[] { "CALO_HIGH", "FIBER_LOW", "PROTEIN_LOW", "FAT_HIGH", "DINNER_HEAVY", "NO_BREAKFAST" })
            Assert.Contains(c, codes);
        Assert.Equal(WeeklyInsightRules.CanhBao, r[0].Loai);
        Assert.Equal("Calo trung bình vượt 30% mục tiêu", r.Single(i => i.Ma == "CALO_HIGH").TieuDe);
        Assert.Contains("2.600", r.Single(i => i.Ma == "CALO_HIGH").NoiDung); // định dạng số tiếng Việt
    }

    [Fact]
    public void Trend_compares_with_previous_week_and_few_days_is_flagged()
    {
        var prev = Week(i => Day(i - 7, 1600, 80, 200, 53, 30, sang: 400, toi: 500));
        var r = WeeklyInsightRules.Build(Week(i => Day(i, 2000, 100, 250, 67, 30, sang: 500, toi: 600), 3), prev);
        var trend = r.Single(i => i.Ma == "TREND");
        Assert.Equal("Calo tăng 25% so với tuần trước", trend.TieuDe);

        var few = WeeklyInsightRules.Build(Week(i => Day(i, 2000, 100, 250, 67, 30, sang: 500), 2), prev);
        Assert.Contains("FEW_DAYS", Codes(few));
        Assert.DoesNotContain("TREND", Codes(few)); // < 3 ngày → không so sánh
    }
}
