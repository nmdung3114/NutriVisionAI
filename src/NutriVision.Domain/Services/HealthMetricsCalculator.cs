using NutriVision.Domain.Constants;

namespace NutriVision.Domain.Services;

public readonly record struct HealthMetrics(decimal Bmi, decimal Bmr, decimal Tdee);

/// <summary>Tính BMI, BMR (Mifflin–St Jeor), TDEE (FR-11, BR-17).</summary>
public static class HealthMetricsCalculator
{
    public const decimal MinHeightCm = 50, MaxHeightCm = 200;
    public const decimal MinWeightKg = 10, MaxWeightKg = 150;
    public const int MinAge = 10, MaxAge = 100;
    public const decimal DefaultCalorieTarget = 2000m;

    public static int TinhTuoi(DateOnly ngaySinh, DateOnly today)
    {
        var age = today.Year - ngaySinh.Year;
        if (ngaySinh > today.AddYears(-age)) age--;
        return age;
    }

    public static HealthMetrics Tinh(string gioiTinh, decimal chieuCaoCm, decimal canNangKg, int tuoi, string mucDoVanDong)
    {
        var m = chieuCaoCm / 100m;
        var bmi = canNangKg / (m * m);

        // Mifflin–St Jeor: BMR = 10W + 6.25H − 5A + s, s = +5 (nam) / −161 (nữ)
        var bmr = 10m * canNangKg + 6.25m * chieuCaoCm - 5m * tuoi + (gioiTinh == GioiTinh.Nam ? 5m : -161m);
        var heSo = MucDoVanDong.HeSo.TryGetValue(mucDoVanDong, out var h) ? h : MucDoVanDong.HeSo[MucDoVanDong.VuaPhai];
        var tdee = bmr * heSo;

        return new HealthMetrics(
            Math.Round(bmi, 2, MidpointRounding.AwayFromZero),
            Math.Round(bmr, 2, MidpointRounding.AwayFromZero),
            Math.Round(tdee, 2, MidpointRounding.AwayFromZero));
    }

    /// <summary>Phân loại BMI theo ngưỡng WHO (châu Á dùng 23/25 nhưng giữ chuẩn WHO chung cho đơn giản).</summary>
    public static string PhanLoaiBmi(decimal bmi) => bmi switch
    {
        < 18.5m => "Gầy",
        < 25m => "Bình thường",
        < 30m => "Thừa cân",
        _ => "Béo phì",
    };
}

/// <summary>Khuyến nghị nước uống: 35 ml/kg cân nặng, làm tròn 50 ml, giới hạn 1500–4000 ml; mặc định 2000 ml khi chưa có cân nặng.</summary>
public static class WaterTarget
{
    public const int DefaultMl = 2000, MinMl = 1500, MaxMl = 4000, MaxPerDayMl = 10000;

    public static int ForWeight(decimal? canNangKg)
    {
        if (canNangKg is not > 0) return DefaultMl;
        var ml = (int)(Math.Round(canNangKg.Value * 35m / 50m, MidpointRounding.AwayFromZero) * 50);
        return Math.Clamp(ml, MinMl, MaxMl);
    }
}
