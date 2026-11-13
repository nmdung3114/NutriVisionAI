using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Dashboard;
using NutriVision.Domain.Services;

namespace NutriVision.Application.Modules.Habits;

/// <summary>Một ô trên lịch tháng. TrangThai = null khi ngày chưa có bữa đã xác nhận.</summary>
public record CalendarDayDto(DateOnly Ngay, decimal Calo, decimal MucTieuCalo, int SoBuaAn, decimal PhanTramMucTieu, string? TrangThai);

public record CalendarDto(int Nam, int Thang, DateOnly HomNay, int SoNgayCoDuLieu, int SoNgayDatChuan, IReadOnlyList<CalendarDayDto> Ngay);

public record BadgeDto(string Ma, string Ten, string MoTa, string BieuTuong, bool DatDuoc, int TienDo, int MucTieu);

public record StreakDto(int ChuoiHienTai, int ChuoiDaiNhat, bool HomNayDaGhi, int TongNgayGhiNhan, int TongBuaAn, IReadOnlyList<BadgeDto> HuyHieu);

public record WeeklyInsightsDto(DateOnly TuNgay, DateOnly DenNgay, int SoNgayCoDuLieu, decimal CaloTrungBinh, IReadOnlyList<InsightDto> NhanXet);

public interface IHabitService
{
    /// <summary>Nhận xét tự động cho 7 ngày gần nhất (so với mục tiêu và tuần trước).</summary>
    Task<WeeklyInsightsDto> GetWeeklyInsightsAsync(CancellationToken ct = default);

    /// <summary>Chuỗi ngày ghi nhận liên tiếp và huy hiệu (tính từ dữ liệu bữa ăn đã xác nhận, không lưu thêm bảng).</summary>
    Task<StreakDto> GetStreakAsync(CancellationToken ct = default);

    /// <summary>Lịch nhiệt theo tháng: mỗi ngày đã qua của tháng kèm calo và trạng thái so với mục tiêu.</summary>
    Task<CalendarDto> GetCalendarAsync(int? nam, int? thang, CancellationToken ct = default);
}

public class HabitService(IApplicationDbContext db, ICurrentUser currentUser, AppClock clock) : IHabitService
{
    public async Task<CalendarDto> GetCalendarAsync(int? namIn, int? thangIn, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var today = clock.Today;
        var nam = namIn ?? today.Year;
        var thang = thangIn ?? today.Month;
        if (thang is < 1 or > 12) throw ValidationAppException.Single("thang", "Tháng phải từ 1 đến 12.");
        if (nam is < 2000 or > 2100) throw ValidationAppException.Single("nam", "Năm không hợp lệ.");

        var first = new DateOnly(nam, thang, 1);
        var last = first.AddMonths(1).AddDays(-1);
        if (last > today) last = today; // không trả ngày tương lai
        var target = await CurrentTargetAsync(userId, ct);

        var rows = first > last
            ? []
            : await db.TongHopDinhDuongNgays.AsNoTracking()
                .Where(t => t.NguoiDungId == userId && t.Ngay >= first && t.Ngay <= last).ToListAsync(ct);
        var byDay = rows.ToDictionary(r => r.Ngay);

        var days = new List<CalendarDayDto>();
        for (var d = first; d <= last; d = d.AddDays(1))
        {
            if (byDay.TryGetValue(d, out var r) && r.SoBuaAnDaGhi > 0)
            {
                var t = r.MucTieuCaloNgay > 0 ? r.MucTieuCaloNgay : target;
                var pct = t > 0 ? Math.Round(r.TongCalo / t * 100m, 1) : 0m;
                days.Add(new CalendarDayDto(d, r.TongCalo, t, r.SoBuaAnDaGhi, pct, DashboardService.TrangThai(pct)));
            }
            else days.Add(new CalendarDayDto(d, 0, target, 0, 0, null));
        }

        return new CalendarDto(nam, thang, today, days.Count(x => x.TrangThai != null), days.Count(x => x.TrangThai == "DatChuan"), days);
    }

    public async Task<StreakDto> GetStreakAsync(CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var today = clock.Today;
        var target = await CurrentTargetAsync(userId, ct);

        var rows = await db.TongHopDinhDuongNgays.AsNoTracking()
            .Where(t => t.NguoiDungId == userId && t.SoBuaAnDaGhi > 0 && t.Ngay <= today)
            .Select(t => new { t.Ngay, t.TongCalo, t.TongChatDam, t.TongChatXo, t.MucTieuCaloNgay, t.SoBuaAnDaGhi })
            .ToListAsync(ct);
        var streak = StreakCalculator.Compute(rows.Select(r => r.Ngay), today);

        var tongBua = rows.Sum(r => r.SoBuaAnDaGhi);
        var datChuan = rows.Count(r =>
        {
            var t = r.MucTieuCaloNgay > 0 ? r.MucTieuCaloNgay : target;
            return t > 0 && DashboardService.TrangThai(r.TongCalo / t * 100m) == "DatChuan";
        });
        var duXo = rows.Count(r => r.TongChatXo >= MacroTargets.FiberTargetGrams);
        var duDam = rows.Count(r => r.TongChatDam >= MacroTargets.TargetGrams(r.MucTieuCaloNgay > 0 ? r.MucTieuCaloNgay : target).ChatDam);
        var buaAi = await db.BuaAns.AsNoTracking().CountAsync(b => b.NguoiDungId == userId && b.DaXacNhan && !b.DaXoa && b.TaoTuAI, ct);
        var ngayDu3Bua = (await db.BuaAns.AsNoTracking()
                .Where(b => b.NguoiDungId == userId && b.DaXacNhan && !b.DaXoa && b.LoaiBuaAn != Domain.Constants.LoaiBuaAn.Phu)
                .Select(b => new { b.NgayAn, b.LoaiBuaAn }).Distinct().ToListAsync(ct))
            .GroupBy(x => x.NgayAn).Count(g => g.Count() >= 3);

        BadgeDto Badge(string ma, string ten, string moTa, string icon, int tienDo, int mucTieu) =>
            new(ma, ten, moTa, icon, tienDo >= mucTieu, Math.Min(tienDo, mucTieu), mucTieu);

        var badges = new List<BadgeDto>
        {
            Badge("FIRST_MEAL", "Khởi đầu", "Ghi nhận bữa ăn đầu tiên", "🍽️", tongBua, 1),
            Badge("STREAK_3", "Khởi động", "Ghi nhận 3 ngày liên tiếp", "🔥", streak.Longest, 3),
            Badge("STREAK_7", "Một tuần bền bỉ", "Ghi nhận 7 ngày liên tiếp", "🏅", streak.Longest, 7),
            Badge("STREAK_30", "Thói quen vàng", "Ghi nhận 30 ngày liên tiếp", "🏆", streak.Longest, 30),
            Badge("ON_TARGET_7", "Cân bằng", "7 ngày đạt mục tiêu calo (90–110%)", "🎯", datChuan, 7),
            Badge("PROTEIN_5", "Giàu đạm", "5 ngày ăn đủ lượng đạm mục tiêu", "💪", duDam, 5),
            Badge("FIBER_5", "Bạn của chất xơ", "5 ngày đạt ≥ 25 g chất xơ", "🥦", duXo, 5),
            Badge("FULL_DAY_3", "Đủ bữa", "3 ngày ăn đủ sáng, trưa, tối", "🍱", ngayDu3Bua, 3),
            Badge("AI_10", "Cộng sự AI", "10 bữa ăn ghi nhận bằng AI", "🤖", buaAi, 10),
        };

        return new StreakDto(streak.Current, streak.Longest, rows.Any(r => r.Ngay == today), rows.Count, tongBua, badges);
    }

    public async Task<WeeklyInsightsDto> GetWeeklyInsightsAsync(CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var today = clock.Today;
        var from = today.AddDays(-6);
        var prevFrom = from.AddDays(-7);
        var target = await CurrentTargetAsync(userId, ct);

        var rows = await db.TongHopDinhDuongNgays.AsNoTracking()
            .Where(t => t.NguoiDungId == userId && t.SoBuaAnDaGhi > 0 && t.Ngay >= prevFrom && t.Ngay <= today).ToListAsync(ct);
        var meals = await db.BuaAns.AsNoTracking()
            .Where(b => b.NguoiDungId == userId && b.DaXacNhan && !b.DaXoa && b.NgayAn >= from && b.NgayAn <= today)
            .Select(b => new { b.NgayAn, b.LoaiBuaAn, b.TongCalo }).ToListAsync(ct);
        var byDayType = meals.GroupBy(m => m.NgayAn)
            .ToDictionary(g => g.Key, g => (IReadOnlyDictionary<string, decimal>)g.GroupBy(m => m.LoaiBuaAn).ToDictionary(x => x.Key, x => x.Sum(m => m.TongCalo)));
        var empty = new Dictionary<string, decimal>();

        InsightDay ToDay(Domain.Entities.TongHopDinhDuongNgay r) =>
            new(r.Ngay, r.Tong, r.MucTieuCaloNgay > 0 ? r.MucTieuCaloNgay : target, byDayType.GetValueOrDefault(r.Ngay) ?? empty);

        var week = rows.Where(r => r.Ngay >= from).OrderBy(r => r.Ngay).Select(ToDay).ToList();
        var prev = rows.Where(r => r.Ngay < from).OrderBy(r => r.Ngay).Select(ToDay).ToList();
        var avg = week.Count == 0 ? 0 : Math.Round(week.Average(d => d.Tong.Calo), 0);
        return new WeeklyInsightsDto(from, today, week.Count, avg, WeeklyInsightRules.Build(week, prev));
    }

    private async Task<decimal> CurrentTargetAsync(Guid userId, CancellationToken ct)
    {
        var p = await db.HoSoNguoiDungs.AsNoTracking().Where(h => h.NguoiDungId == userId).Select(h => (decimal?)h.MucTieuCalo).FirstOrDefaultAsync(ct);
        return p is > 0 ? p.Value : HealthMetricsCalculator.DefaultCalorieTarget;
    }
}
