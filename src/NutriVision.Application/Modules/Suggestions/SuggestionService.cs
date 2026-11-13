using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Foods;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;

namespace NutriVision.Application.Modules.Suggestions;

public record SuggestionDto(FoodDto Food, string LyDo);

public interface ISuggestionService
{
    Task<IReadOnlyList<SuggestionDto>> GetAsync(CancellationToken ct = default);
}

/// <summary>Gợi ý món dựa trên lịch sử 30 ngày + mục tiêu calo; chỉ chọn từ bảng tham chiếu (FR-42, BR-42).</summary>
public class SuggestionService(IApplicationDbContext db, ICurrentUser currentUser, AppClock clock, IFoodCatalog catalog) : ISuggestionService
{
    public async Task<IReadOnlyList<SuggestionDto>> GetAsync(CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var since = clock.Today.AddDays(-30);

        var hs = await db.HoSoNguoiDungs.AsNoTracking().FirstOrDefaultAsync(h => h.NguoiDungId == userId, ct);
        var target = hs?.MucTieuCalo is > 0 ? hs.MucTieuCalo : HealthMetricsCalculator.DefaultCalorieTarget;
        // Mục tiêu < TDEE (−5%) ⇒ giảm cân; > TDEE (+5%) ⇒ tăng cân; còn lại duy trì.
        var goal = hs is null || hs.ChiSoTDEE <= 0 ? "Duytri"
            : target < hs.ChiSoTDEE * 0.95m ? "Giam"
            : target > hs.ChiSoTDEE * 1.05m ? "Tang" : "Duytri";

        var foods = await catalog.GetActiveAsync(ct);

        var recentNames = await db.ThanhPhanBuaAns.AsNoTracking()
            .Where(t => t.BuaAn!.NguoiDungId == userId && t.BuaAn.DaXacNhan && !t.BuaAn.DaXoa && t.BuaAn.NgayAn >= since)
            .Select(t => new { t.ThucPhamThamChieuId, t.TenThanhPhan })
            .ToListAsync(ct);
        var eatenIds = recentNames.Where(x => x.ThucPhamThamChieuId.HasValue).Select(x => x.ThucPhamThamChieuId!.Value).ToHashSet();
        var eatenGroups = foods.Where(f => eatenIds.Contains(f.Id)).GroupBy(f => f.NhomThucPham)
            .ToDictionary(g => g.Key, g => g.Count());

        var scored = foods.Select(f => (Food: f, Score: Score(f, goal, eatenIds, eatenGroups), Reason: Reason(f, goal, eatenIds, eatenGroups)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score).ThenBy(x => x.Food.TenThucPham)
            .Take(6)
            .Select(x => new SuggestionDto(FoodDto.From(x.Food), x.Reason))
            .ToList();
        return scored;
    }

    private static double Score(ThucPhamThamChieu f, string goal, HashSet<Guid> eaten, Dictionary<string, int> groups)
    {
        var cal = (double)f.CaloTren100g;
        var protein = (double)f.ChatDamTren100g;
        var fiber = (double)f.ChatXoTren100g;
        var fat = (double)f.ChatBeoTren100g;
        var proteinPerKcal = cal > 0 ? protein / cal * 100 : 0;

        var s = goal switch
        {
            "Giam" => 60 - cal * 0.18 + proteinPerKcal * 1.2 + fiber * 2.5 - fat * 0.6,
            "Tang" => cal * 0.08 + protein * 1.2 + 10,
            _ => 30 + proteinPerKcal * 0.5 + fiber * 1.5 - Math.Abs(cal - 150) * 0.05,
        };
        if (groups.TryGetValue(f.NhomThucPham, out var n)) s += Math.Min(n, 5) * 1.5; // hợp khẩu vị đã ăn
        if (eaten.Contains(f.Id)) s -= 3;                                              // ưu tiên đa dạng
        return s;
    }

    private static string Reason(ThucPhamThamChieu f, string goal, HashSet<Guid> eaten, Dictionary<string, int> groups)
    {
        var parts = new List<string>();
        switch (goal)
        {
            case "Giam": parts.Add($"Ít năng lượng ({f.CaloTren100g:0} kcal/100g)"); break;
            case "Tang": parts.Add($"Giàu năng lượng ({f.CaloTren100g:0} kcal/100g)"); break;
            default: parts.Add($"Cân đối ({f.CaloTren100g:0} kcal/100g)"); break;
        }
        if (f.ChatDamTren100g >= 8) parts.Add($"giàu đạm ({f.ChatDamTren100g:0.#} g)");
        if (f.ChatXoTren100g >= 2) parts.Add($"nhiều chất xơ ({f.ChatXoTren100g:0.#} g)");
        var tail = goal switch { "Giam" => "phù hợp mục tiêu giảm cân", "Tang" => "phù hợp mục tiêu tăng cân", _ => "phù hợp mục tiêu duy trì cân nặng" };
        if (groups.ContainsKey(f.NhomThucPham)) tail += $", cùng nhóm \"{f.NhomThucPham}\" bạn hay ăn";
        return string.Join(", ", parts) + " — " + tail;
    }
}
