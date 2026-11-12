using NutriVision.Domain.ValueObjects;

namespace NutriVision.Domain.Services;

public readonly record struct MacroShare(decimal ChatDam, decimal TinhBot, decimal ChatBeo);

/// <summary>Mục tiêu đa lượng chất (20% đạm / 50% tinh bột / 30% béo năng lượng) và tỷ lệ năng lượng thực tế.</summary>
public static class MacroTargets
{
    public const decimal ProteinShare = 0.20m, CarbShare = 0.50m, FatShare = 0.30m;
    public const decimal FiberTargetGrams = 25m;

    public static MacroShare TargetGrams(decimal targetCalories) => new(
        Math.Round(targetCalories * ProteinShare / 4m, 1),
        Math.Round(targetCalories * CarbShare / 4m, 1),
        Math.Round(targetCalories * FatShare / 9m, 1));

    /// <summary>Tỷ lệ % năng lượng từ đạm/tinh bột/béo (4-4-9 kcal/g); 0 nếu chưa có dữ liệu.</summary>
    public static MacroShare EnergyPercent(NutritionValues n)
    {
        var p = n.ChatDam * 4m; var c = n.TinhBot * 4m; var f = n.ChatBeo * 9m;
        var sum = p + c + f;
        if (sum <= 0) return new MacroShare(0, 0, 0);
        return new MacroShare(Math.Round(p / sum * 100m, 1), Math.Round(c / sum * 100m, 1), Math.Round(f / sum * 100m, 1));
    }

    /// <summary>Đánh giá mức so với mục tiêu: Thap (&lt;85%), DatChuan, Cao (&gt;115%).</summary>
    public static string DanhGia(decimal actual, decimal target, decimal lower = 0.85m, decimal upper = 1.15m)
    {
        if (target <= 0) return "DatChuan";
        var r = actual / target;
        return r < lower ? "Thap" : r > upper ? "Cao" : "DatChuan";
    }
}
