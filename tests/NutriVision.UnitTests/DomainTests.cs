using NutriVision.Application.Modules.Auth;
using NutriVision.Application.Modules.Foods;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;
using NutriVision.Domain.ValueObjects;
using Xunit;

namespace NutriVision.UnitTests;

public class HealthMetricsTests
{
    [Fact]
    public void Mifflin_StJeor_Male_Matches_Manual_Calculation()
    {
        // 10*68 + 6.25*172 - 5*25 + 5 = 1635 ; TDEE = 1635 * 1.55 = 2534.25 ; BMI = 68 / 1.72^2 = 22.99
        var m = HealthMetricsCalculator.Tinh(GioiTinh.Nam, 172, 68, 25, MucDoVanDong.VuaPhai);
        Assert.Equal(1635m, m.Bmr);
        Assert.Equal(2534.25m, m.Tdee);
        Assert.Equal(22.99m, m.Bmi);
    }

    [Fact]
    public void Female_Uses_Minus161_Offset()
    {
        var m = HealthMetricsCalculator.Tinh(GioiTinh.Nu, 160, 50, 30, MucDoVanDong.ItVanDong);
        Assert.Equal(10 * 50 + 6.25m * 160 - 5 * 30 - 161, m.Bmr);
        Assert.Equal(Math.Round(m.Bmr * 1.2m, 2), m.Tdee);
    }

    [Fact] // AC-18
    public void Changing_Weight_Recalculates_All_Metrics()
    {
        var a = HealthMetricsCalculator.Tinh(GioiTinh.Nam, 170, 60, 30, MucDoVanDong.VuaPhai);
        var b = HealthMetricsCalculator.Tinh(GioiTinh.Nam, 170, 65, 30, MucDoVanDong.VuaPhai);
        Assert.True(b.Bmi > a.Bmi && b.Bmr > a.Bmr && b.Tdee > a.Tdee);
    }

    [Theory]
    [InlineData(17, "Gầy")] [InlineData(22, "Bình thường")] [InlineData(27, "Thừa cân")] [InlineData(32, "Béo phì")]
    public void Bmi_Classification(double bmi, string expected) => Assert.Equal(expected, HealthMetricsCalculator.PhanLoaiBmi((decimal)bmi));

    [Fact]
    public void Age_Handles_Birthday_Not_Yet_Reached()
    {
        Assert.Equal(24, HealthMetricsCalculator.TinhTuoi(new DateOnly(2000, 12, 31), new DateOnly(2025, 6, 1)));
        Assert.Equal(25, HealthMetricsCalculator.TinhTuoi(new DateOnly(2000, 6, 1), new DateOnly(2025, 6, 1)));
    }
}

public class NutritionCalculatorTests
{
    private static ThucPhamThamChieu Food(string unit, decimal std, decimal kcal) =>
        new() { TenThucPham = "x", DonViChuan = unit, KhoiLuongChuan = std, CaloTren100g = kcal, ChatDamTren100g = 10, TinhBotTren100g = 20, ChatBeoTren100g = 5, ChatXoTren100g = 2 };

    [Fact]
    public void Gram_Is_Identity_And_Nutrition_Scales_Per_100g()
    {
        var f = Food("tô", 450, 108);
        var gram = NutritionCalculator.QuyDoiGram(200, "gram", f);
        Assert.Equal(200m, gram);
        var n = NutritionCalculator.TinhTheoGram(f.DinhDuongTren100g, gram!.Value);
        Assert.Equal(216m, n.Calo);
        Assert.Equal(20m, n.ChatDam);
    }

    [Fact]
    public void Food_Own_Unit_Uses_Standard_Weight()
    {
        var f = Food("tô", 450, 108);
        Assert.Equal(450m, NutritionCalculator.QuyDoiGram(1, "Tô", f));
        Assert.Equal(675m, NutritionCalculator.QuyDoiGram(1.5m, "to", f));
    }

    [Fact]
    public void Generic_Unit_Falls_Back_To_Default_Table_And_Unknown_Is_Null()
    {
        Assert.Equal(400m, NutritionCalculator.QuyDoiGram(2, "bát", null));
        Assert.Null(NutritionCalculator.QuyDoiGram(1, "xyz", null));
    }

    [Fact] // AC-36
    public void Total_Is_Sum_Of_Components()
    {
        var total = NutritionCalculator.Tong([new(100, 1, 2, 3, 4), new(50.5m, 1, 1, 1, 1), new(25.25m, 0, 0, 0, 0)]);
        Assert.Equal(175.75m, total.Calo);
        Assert.Equal(2m, total.ChatDam);
    }
}

public class TextAndMatchingTests
{
    [Theory]
    [InlineData("Phở Bò Tái", "pho bo tai")] [InlineData("  Đậu hũ  chiên!! ", "dau hu chien")] [InlineData("Cơm-tấm", "com tam")]
    public void Normalizer_Strips_Diacritics(string input, string expected) => Assert.Equal(expected, TextNormalizer.Chuan(input));

    [Fact]
    public void Matcher_Finds_Close_Name_And_Rejects_Unknown()
    {
        var foods = new[]
        {
            new ThucPhamThamChieu { TenThucPham = "Cơm trắng gạo tẻ" }, new ThucPhamThamChieu { TenThucPham = "Phở bò tái nạm" },
            new ThucPhamThamChieu { TenThucPham = "Cá kho tộ" },
        };
        Assert.Equal("Cơm trắng gạo tẻ", FoodMatcher.BestAutoMatch("cơm trắng", foods)?.TenThucPham);
        Assert.Equal("Phở bò tái nạm", FoodMatcher.BestAutoMatch("Phở bò", foods)?.TenThucPham);
        Assert.Null(FoodMatcher.BestAutoMatch("Món lạ hoắc", foods)); // BR-30
    }
}

public class PasswordPolicyTests
{
    [Theory] // BR-10 / AC-03
    [InlineData("abc", false)] [InlineData("abcdefgh", false)] [InlineData("12345678", false)] [InlineData("Abc12345", true)] [InlineData("matkhau1", true)]
    public void Policy(string pw, bool ok) => Assert.Equal(ok, PasswordPolicy.IsValid(pw));
}

public class UserAggregateTests
{
    [Fact] // BR-07
    public void Locks_After_Five_Failures()
    {
        var now = DateTimeOffset.UtcNow;
        var u = NguoiDung.TaoDaKichHoat("a@b.c", "A", "h", VaiTro.User, now);
        for (var i = 0; i < 4; i++) Assert.False(u.GhiNhanDangNhapSai(now, 5, TimeSpan.FromMinutes(15)));
        Assert.True(u.GhiNhanDangNhapSai(now, 5, TimeSpan.FromMinutes(15)));
        Assert.True(u.DangBiKhoaTamThoi(now.AddMinutes(14)));
        Assert.False(u.DangBiKhoaTamThoi(now.AddMinutes(16)));
    }

    [Fact]
    public void Only_Confirmed_NotDeleted_Meals_Count()
    {
        var b = new BuaAn();
        Assert.False(b.TinhVaoTongHop);
        b.XacNhan(DateTimeOffset.UtcNow);
        Assert.True(b.TinhVaoTongHop);
        b.XoaMem(DateTimeOffset.UtcNow);
        Assert.False(b.TinhVaoTongHop);
    }

    [Fact]
    public void MacroTargets_Energy_Split()
    {
        var s = MacroTargets.EnergyPercent(new NutritionValues(0, 25, 50, 11.11m, 0)); // ≈100 + 200 + 100 kcal
        Assert.Equal(25m, s.ChatDam, 0);
        Assert.Equal(50m, s.TinhBot, 0);
    }
}

public class StreakCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 12);
    private static IEnumerable<DateOnly> Days(params int[] offsets) => offsets.Select(o => Today.AddDays(-o));

    [Fact]
    public void Empty_history_has_no_streak() =>
        Assert.Equal(new StreakResult(0, 0, null), StreakCalculator.Compute([], Today));

    [Fact]
    public void Current_streak_counts_back_from_today()
    {
        var r = StreakCalculator.Compute(Days(0, 1, 2, 4, 5), Today);
        Assert.Equal(3, r.Current);
        Assert.Equal(3, r.Longest);
    }

    [Fact]
    public void Streak_ending_yesterday_is_kept_until_today_ends()
    {
        Assert.Equal(2, StreakCalculator.Compute(Days(1, 2), Today).Current);
        Assert.Equal(0, StreakCalculator.Compute(Days(2, 3), Today).Current); // đứt hôm qua
    }

    [Fact]
    public void Longest_streak_is_found_in_the_past_and_ignores_future_and_duplicates()
    {
        var r = StreakCalculator.Compute(Days(0, 10, 11, 12, 13, 13, -1, -2), Today);
        Assert.Equal(1, r.Current);
        Assert.Equal(4, r.Longest);
        Assert.Equal(Today.AddDays(-10), r.LongestEnd);
    }
}

public class WaterTargetTests
{
    [Theory]
    [InlineData(null, 2000)]
    [InlineData(0, 2000)]
    [InlineData(55, 1950)]   // 1925 → 1950
    [InlineData(68, 2400)]   // 2380 → 2400
    [InlineData(30, 1500)]   // dưới mức tối thiểu
    [InlineData(150, 4000)]  // trên mức tối đa
    public void Target_is_35ml_per_kg_rounded_and_clamped(int? kg, int expected) =>
        Assert.Equal(expected, WaterTarget.ForWeight(kg));
}
