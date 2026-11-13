using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Foods;
using NutriVision.Application.Modules.Meals;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Services;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Application.Modules.Dashboard;

public record TodaySummaryDto(decimal Calo, decimal ChatDam, decimal TinhBot, decimal ChatBeo, decimal ChatXo, int SoBuaAn, decimal PhanTramMucTieu, string TrangThai);
public record MacroGramsDto(decimal ChatDam, decimal TinhBot, decimal ChatBeo);
public record MealTypeTotalDto(string LoaiBuaAn, decimal Calo, int SoBua);
public record DayPointDto(DateOnly Ngay, decimal Calo, decimal MucTieuCalo, int SoBuaAn);

public record DashboardDto(
    DateOnly Ngay, decimal MucTieuCalo, bool HoSoDayDu, bool CoDuLieu,
    TodaySummaryDto HomNay, MacroGramsDto MucTieuMacro, MacroGramsDto TyLeMacro,
    IReadOnlyList<MealSummaryDto> BuaAnHomNay, IReadOnlyList<MealTypeTotalDto> TheoLoaiBuaAn, IReadOnlyList<DayPointDto> BayNgay);

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken ct = default);
}

public class DashboardService(IApplicationDbContext db, ICurrentUser currentUser, AppClock clock, MealMapper mapper) : IDashboardService
{
    public static string TrangThai(decimal phanTram) => phanTram > 110m ? "Vuot" : phanTram >= 90m ? "DatChuan" : "Thieu";

    public async Task<DashboardDto> GetAsync(CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var today = clock.Today;
        var start = today.AddDays(-6);

        var profile = await db.HoSoNguoiDungs.AsNoTracking().Where(h => h.NguoiDungId == userId).Select(h => (decimal?)h.MucTieuCalo).FirstOrDefaultAsync(ct);
        var hoSoDayDu = profile.HasValue;
        var target = profile is > 0 ? profile.Value : HealthMetricsCalculator.DefaultCalorieTarget;

        var rows = await db.TongHopDinhDuongNgays.AsNoTracking()
            .Where(t => t.NguoiDungId == userId && t.Ngay >= start && t.Ngay <= today).ToListAsync(ct);

        var homNayRow = rows.FirstOrDefault(r => r.Ngay == today);
        var n = homNayRow?.Tong ?? NutritionValues.Zero;
        var pct = target > 0 ? Math.Round(n.Calo / target * 100m, 1) : 0m;

        var meals = await db.BuaAns.AsNoTracking().Include(b => b.ThanhPhans)
            .Where(b => b.NguoiDungId == userId && b.NgayAn == today && b.DaXacNhan && !b.DaXoa)
            .OrderBy(b => b.ThoiGianGhiNhan).ToListAsync(ct);

        var theoLoai = LoaiBuaAn.All.Select(l =>
        {
            var g = meals.Where(m => m.LoaiBuaAn == l).ToList();
            return new MealTypeTotalDto(l, g.Sum(m => m.TongCalo), g.Count);
        }).ToList();

        var bayNgay = Enumerable.Range(0, 7).Select(i =>
        {
            var d = start.AddDays(i);
            var r = rows.FirstOrDefault(x => x.Ngay == d);
            return new DayPointDto(d, r?.TongCalo ?? 0m, r is { MucTieuCaloNgay: > 0 } ? r.MucTieuCaloNgay : target, r?.SoBuaAnDaGhi ?? 0);
        }).ToList();

        var coDuLieu = rows.Count > 0 || await db.BuaAns.AsNoTracking().AnyAsync(b => b.NguoiDungId == userId && b.DaXacNhan && !b.DaXoa, ct);
        var goals = MacroTargets.TargetGrams(target);
        var share = MacroTargets.EnergyPercent(n);

        return new DashboardDto(
            today, target, hoSoDayDu, coDuLieu,
            new TodaySummaryDto(n.Calo, n.ChatDam, n.TinhBot, n.ChatBeo, n.ChatXo, homNayRow?.SoBuaAnDaGhi ?? 0, pct, TrangThai(pct)),
            new MacroGramsDto(goals.ChatDam, goals.TinhBot, goals.ChatBeo),
            new MacroGramsDto(share.ChatDam, share.TinhBot, share.ChatBeo),
            meals.Select(mapper.ToSummary).ToList(), theoLoai, bayNgay);
    }
}
