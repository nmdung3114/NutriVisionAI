using NutriVision.Domain.Entities;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Domain.Services;

/// <summary>Quy đổi khẩu phần ra gram và tính dinh dưỡng thành phần (FR-28, FR-29, AC-36).</summary>
public static class NutritionCalculator
{
    /// <summary>Bảng quy đổi mặc định (gram trên 1 đơn vị) khi đơn vị không trùng đơn vị chuẩn của thực phẩm.</summary>
    public static readonly IReadOnlyDictionary<string, decimal> DefaultUnitGrams = new Dictionary<string, decimal>
    {
        ["g"] = 1m,
        ["ml"] = 1m,
        ["kg"] = 1000m,
        ["lit"] = 1000m,
        ["bat"] = 200m,
        ["chen"] = 150m,
        ["dia"] = 250m,
        ["to"] = 400m,
        ["mieng"] = 50m,
        ["qua"] = 50m,
        ["cai"] = 100m,
        ["cuon"] = 50m,
        ["lat"] = 20m,
        ["muong"] = 15m,
        ["ly"] = 240m,
        ["phan"] = 200m,
        ["o"] = 160m,
    };

    /// <summary>Tên hiển thị của từng đơn vị mặc định (khóa đã bỏ dấu → nhãn tiếng Việt).</summary>
    public static readonly IReadOnlyDictionary<string, string> UnitLabels = new Dictionary<string, string>
    {
        ["g"] = "g", ["ml"] = "ml", ["kg"] = "kg", ["lit"] = "lít", ["bat"] = "bát", ["chen"] = "chén", ["dia"] = "đĩa",
        ["to"] = "tô", ["mieng"] = "miếng", ["qua"] = "quả", ["cai"] = "cái", ["cuon"] = "cuốn", ["lat"] = "lát",
        ["muong"] = "muỗng", ["ly"] = "ly", ["phan"] = "phần", ["o"] = "ổ",
    };

    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["gram"] = "g", ["gam"] = "g", ["grams"] = "g", ["l"] = "lit", ["litre"] = "lit", ["dĩa"] = "dia",
        ["tô"] = "to", ["thia"] = "muong", ["thìa"] = "muong", ["trai"] = "qua", ["chiec"] = "cai",
    };

    public static string ChuanDonVi(string? donVi)
    {
        var key = TextNormalizer.Chuan(donVi);
        return Aliases.TryGetValue(key, out var a) ? a : key;
    }

    /// <summary>Trả về số gram trên 1 đơn vị, hoặc null nếu không quy đổi được.</summary>
    public static decimal? GramMoiDonVi(string? donVi, ThucPhamThamChieu? food)
    {
        var key = ChuanDonVi(donVi);
        if (key.Length == 0) return null;
        if (key is "g" or "ml" or "kg" or "lit") return DefaultUnitGrams[key];
        if (food is not null && ChuanDonVi(food.DonViChuan) == key && food.KhoiLuongChuan > 0) return food.KhoiLuongChuan;
        return DefaultUnitGrams.TryGetValue(key, out var g) ? g : null;
    }

    /// <summary>Khối lượng (gram) = khẩu phần × gram/đơn vị.</summary>
    public static decimal? QuyDoiGram(decimal khauPhan, string? donVi, ThucPhamThamChieu? food)
    {
        var per = GramMoiDonVi(donVi, food);
        return per is null ? null : Math.Round(khauPhan * per.Value, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Dinh dưỡng = giá trị trên 100g × gram / 100.</summary>
    public static NutritionValues TinhTheoGram(NutritionValues tren100g, decimal gram) => (tren100g * (gram / 100m)).Round();

    public static NutritionValues Tong(IEnumerable<NutritionValues> items)
    {
        var total = NutritionValues.Zero;
        foreach (var i in items) total += i;
        return total.Round();
    }
}
