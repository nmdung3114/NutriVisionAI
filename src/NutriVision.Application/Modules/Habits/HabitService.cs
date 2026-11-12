using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Dashboard;
using NutriVision.Domain.Services;

namespace NutriVision.Application.Modules.Habits;

/// <summary>Một ô trên lịch tháng. TrangThai = null khi ngày chưa có bữa đã xác nhận.</summary>
public record CalendarDayDto(DateOnly Ngay, decimal Calo, decimal MucTieuCalo, int SoBuaAn, decimal PhanTramMucTieu, string? TrangThai);

public record CalendarDto(int Nam, int Thang, DateOnly HomNay, int SoNgayCoDuLieu, int SoNgayDatChuan, IReadOnlyList<CalendarDayDto> Ngay);

public interface IHabitService
{
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

    private async Task<decimal> CurrentTargetAsync(Guid userId, CancellationToken ct)
    {
        var p = await db.HoSoNguoiDungs.AsNoTracking().Where(h => h.NguoiDungId == userId).Select(h => (decimal?)h.MucTieuCalo).FirstOrDefaultAsync(ct);
        return p is > 0 ? p.Value : HealthMetricsCalculator.DefaultCalorieTarget;
    }
}
