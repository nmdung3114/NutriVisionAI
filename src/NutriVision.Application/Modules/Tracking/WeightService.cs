using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Audit;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;

namespace NutriVision.Application.Modules.Tracking;

public record WeightEntryDto(Guid Id, DateOnly Ngay, decimal CanNang, string? GhiChu);

public record WeightOverviewDto(
    decimal? HienTai, decimal? BanDau, decimal? ThayDoi7Ngay, decimal? ThayDoi30Ngay,
    decimal? ChieuCao, decimal? Bmi, string? PhanLoaiBmi, IReadOnlyList<WeightEntryDto> DanhSach);

public record LogWeightRequest(DateOnly? Ngay, decimal CanNang, string? GhiChu);

public class LogWeightRequestValidator : AbstractValidator<LogWeightRequest>
{
    public LogWeightRequestValidator(AppClock clock)
    {
        RuleFor(x => x.CanNang).InclusiveBetween(10, 150).WithMessage("Cân nặng phải trong khoảng 10–150 kg.");
        RuleFor(x => x.GhiChu).MaximumLength(200).WithMessage("Ghi chú tối đa 200 ký tự.");
        RuleFor(x => x.Ngay!.Value)
            .Must(d => d <= clock.Today).WithMessage("Ngày không được ở tương lai.")
            .Must(d => d >= clock.Today.AddYears(-2)).WithMessage("Ngày quá xa trong quá khứ.")
            .When(x => x.Ngay.HasValue);
    }
}

public interface IWeightService
{
    Task<WeightOverviewDto> GetAsync(int days, CancellationToken ct = default);
    /// <summary>Ghi cân nặng (1 bản ghi/ngày, ghi lại cùng ngày thì cập nhật). Bản ghi mới nhất được đồng bộ vào hồ sơ.</summary>
    Task<WeightOverviewDto> LogAsync(LogWeightRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public class WeightService(IApplicationDbContext db, ICurrentUser currentUser, AppClock clock, IAuditService audit) : IWeightService
{
    public async Task<WeightOverviewDto> GetAsync(int days, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        days = Math.Clamp(days, 7, 730);
        var today = clock.Today;
        var from = today.AddDays(-(days - 1));

        var list = await db.NhatKyCanNangs.AsNoTracking()
            .Where(w => w.NguoiDungId == userId && w.Ngay >= from && w.Ngay <= today)
            .OrderBy(w => w.Ngay)
            .Select(w => new WeightEntryDto(w.Id, w.Ngay, w.CanNang, w.GhiChu))
            .ToListAsync(ct);
        var latest = await db.NhatKyCanNangs.AsNoTracking().Where(w => w.NguoiDungId == userId)
            .OrderByDescending(w => w.Ngay).Select(w => (decimal?)w.CanNang).FirstOrDefaultAsync(ct);
        var profile = await db.HoSoNguoiDungs.AsNoTracking().FirstOrDefaultAsync(h => h.NguoiDungId == userId, ct);

        var current = latest ?? profile?.CanNang;
        decimal? Change(int back)
        {
            if (latest is null) return null;
            var since = today.AddDays(-back);
            var baseline = list.Where(w => w.Ngay <= since).LastOrDefault() ?? list.FirstOrDefault(w => w.Ngay > since);
            return baseline is null || list.Count < 2 ? null : Math.Round(latest.Value - baseline.CanNang, 2);
        }

        decimal? bmi = null;
        if (current is { } kg && profile is { ChieuCao: > 0 })
            bmi = HealthMetricsCalculator.Tinh(profile.GioiTinh, profile.ChieuCao, kg, HealthMetricsCalculator.TinhTuoi(profile.NgaySinh, today), profile.MucDoVanDong).Bmi;

        return new WeightOverviewDto(current, list.FirstOrDefault()?.CanNang, Change(7), Change(30), profile?.ChieuCao, bmi,
            bmi is { } b ? HealthMetricsCalculator.PhanLoaiBmi(b) : null, list);
    }

    public async Task<WeightOverviewDto> LogAsync(LogWeightRequest r, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var ngay = r.Ngay ?? clock.Today;
        var now = clock.UtcNow;
        var kg = Math.Round(r.CanNang, 2);

        var entry = await db.NhatKyCanNangs.FirstOrDefaultAsync(w => w.NguoiDungId == userId && w.Ngay == ngay, ct);
        if (entry is null)
        {
            entry = new NhatKyCanNang { NguoiDungId = userId, Ngay = ngay, NgayTao = now };
            db.NhatKyCanNangs.Add(entry);
        }
        entry.CanNang = kg;
        entry.GhiChu = string.IsNullOrWhiteSpace(r.GhiChu) ? null : r.GhiChu.Trim();
        entry.NgayCapNhat = now;

        // Bản ghi mới nhất → cập nhật hồ sơ: tính lại BMI/BMR/TDEE. Mục tiêu calo chỉ đổi theo khi
        // người dùng đang dùng mục tiêu mặc định (= TDEE cũ); mục tiêu tự đặt được giữ nguyên.
        var newerExists = await db.NhatKyCanNangs.AnyAsync(w => w.NguoiDungId == userId && w.Ngay > ngay, ct);
        var hs = newerExists ? null : await db.HoSoNguoiDungs.FirstOrDefaultAsync(h => h.NguoiDungId == userId, ct);
        if (hs is not null)
        {
            var dungMacDinh = hs.MucTieuCalo == Math.Round(hs.ChiSoTDEE, 0, MidpointRounding.AwayFromZero);
            var m = HealthMetricsCalculator.Tinh(hs.GioiTinh, hs.ChieuCao, kg, HealthMetricsCalculator.TinhTuoi(hs.NgaySinh, clock.Today), hs.MucDoVanDong);
            hs.CanNang = kg;
            hs.ChiSoBMI = m.Bmi;
            hs.ChiSoBMR = m.Bmr;
            hs.ChiSoTDEE = m.Tdee;
            if (dungMacDinh)
            {
                hs.MucTieuCalo = Math.Round(m.Tdee, 0, MidpointRounding.AwayFromZero);
                var homNay = await db.TongHopDinhDuongNgays.FirstOrDefaultAsync(t => t.NguoiDungId == userId && t.Ngay == clock.Today, ct);
                if (homNay is not null) { homNay.MucTieuCaloNgay = hs.MucTieuCalo; homNay.NgayCapNhat = now; }
            }
            hs.NgayCapNhat = now;
        }

        audit.Ghi(HanhDong.GhiCanNang, "NhatKyCanNang", entry.Id.ToString(), $"Ghi cân nặng {kg} kg ngày {ngay:dd/MM/yyyy}");
        await db.SaveChangesAsync(ct);
        return await GetAsync(90, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var entry = await db.NhatKyCanNangs.FirstOrDefaultAsync(w => w.Id == id, ct) ?? throw new NotFoundException("Không tìm thấy bản ghi cân nặng.");
        if (entry.NguoiDungId != userId) throw new ForbiddenException("Bạn không có quyền xóa bản ghi này.");
        db.NhatKyCanNangs.Remove(entry);
        await db.SaveChangesAsync(ct);
    }
}
