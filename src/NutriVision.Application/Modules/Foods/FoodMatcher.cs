using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;

namespace NutriVision.Application.Modules.Foods;

/// <summary>So khớp tên thực phẩm gần đúng, không phân biệt dấu/hoa thường (BR-29, BR-30, AC-37).</summary>
public static class FoodMatcher
{
    public const double AutoAcceptScore = 0.75;

    public static double Score(string query, string candidate)
    {
        var q = TextNormalizer.Chuan(query);
        var c = TextNormalizer.Chuan(candidate);
        if (q.Length == 0 || c.Length == 0) return 0;
        if (q == c) return 1.0;

        var (shorter, longer) = q.Length <= c.Length ? (q, c) : (c, q);
        if ((" " + longer + " ").Contains(" " + shorter + " "))
            return 0.7 + 0.3 * shorter.Length / (double)longer.Length;

        var qt = q.Split(' ').ToHashSet();
        var ct = c.Split(' ').ToHashSet();
        var inter = qt.Intersect(ct).Count();
        if (inter == 0) return 0;
        var jaccard = inter / (double)qt.Union(ct).Count();
        var containment = inter / (double)Math.Min(qt.Count, ct.Count);
        return Math.Min(0.69, 0.5 * jaccard + 0.4 * containment);
    }

    public static IReadOnlyList<(ThucPhamThamChieu Food, double Score)> Match(string name, IEnumerable<ThucPhamThamChieu> foods, int take = 5, double minScore = 0.3) =>
        foods.Select(f => (Food: f, Score: Score(name, f.TenThucPham)))
            .Where(x => x.Score >= minScore)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Food.TenThucPham.Length)
            .Take(take)
            .ToList();

    /// <summary>Chọn thực phẩm khớp đủ tin cậy để tự gán; null nếu không có (không "bịa" dữ liệu – BR-30).</summary>
    public static ThucPhamThamChieu? BestAutoMatch(string name, IEnumerable<ThucPhamThamChieu> foods) =>
        Match(name, foods, 1, AutoAcceptScore).Select(x => x.Food).FirstOrDefault();
}
