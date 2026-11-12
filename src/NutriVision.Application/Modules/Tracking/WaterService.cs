using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;

namespace NutriVision.Application.Modules.Tracking;

public record WaterDayDto(DateOnly Ngay, int LuongMl);

public record WaterSummaryDto(DateOnly Ngay, int LuongMl, int MucTieuMl, decimal PhanTram, int SoNgayDatMucTieu7Ngay, IReadOnlyList<WaterDayDto> BayNgay);

/// <summary>Cộng (hoặc trừ khi hoàn tác) lượng nước vào một ngày.</summary>
public record AddWaterRequest(int Ml, DateOnly? Ngay);

public class AddWaterRequestValidator : AbstractValidator<AddWaterRequest>
{
    public AddWaterRequestValidator(AppClock clock)
    {
        RuleFor(x => x.Ml).NotEqual(0).WithMessage("Lượng nước phải khác 0.")
            .InclusiveBetween(-2000, 2000).WithMessage("Mỗi lần ghi từ -2000 đến 2000 ml.");
        RuleFor(x => x.Ngay!.Value)
            .Must(d => d <= clock.Today).WithMessage("Ngày không được ở tương lai.")
            .Must(d => d >= clock.Today.AddDays(-30)).WithMessage("Chỉ được ghi bù trong 30 ngày gần nhất.")
            .When(x => x.Ngay.HasValue);
    }
}

public interface IWaterService
{
    Task<WaterSummaryDto> GetAsync(DateOnly? ngay, CancellationToken ct = default);
    Task<WaterSummaryDto> AddAsync(AddWaterRequest request, CancellationToken ct = default);
}

public class WaterService(IApplicationDbContext db, ICurrentUser currentUser, AppClock clock) : IWaterService
{
    public async Task<WaterSummaryDto> GetAsync(DateOnly? ngayIn, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var ngay = ngayIn ?? clock.Today;
        if (ngay > clock.Today) throw ValidationAppException.Single("ngay", "Ngày không được ở tương lai.");
        var from = ngay.AddDays(-6);

        var rows = await db.NhatKyNuocUongs.AsNoTracking()
            .Where(w => w.NguoiDungId == userId && w.Ngay >= from && w.Ngay <= ngay)
            .ToDictionaryAsync(w => w.Ngay, w => w.LuongMl, ct);
        var canNang = await db.HoSoNguoiDungs.AsNoTracking().Where(h => h.NguoiDungId == userId).Select(h => (decimal?)h.CanNang).FirstOrDefaultAsync(ct);
        var target = WaterTarget.ForWeight(canNang);

        var week = Enumerable.Range(0, 7).Select(i => from.AddDays(i)).Select(d => new WaterDayDto(d, rows.GetValueOrDefault(d))).ToList();
        var today = rows.GetValueOrDefault(ngay);
        return new WaterSummaryDto(ngay, today, target, Math.Round(today * 100m / target, 1), week.Count(d => d.LuongMl >= target), week);
    }

    public async Task<WaterSummaryDto> AddAsync(AddWaterRequest r, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var ngay = r.Ngay ?? clock.Today;
        var entry = await db.NhatKyNuocUongs.FirstOrDefaultAsync(w => w.NguoiDungId == userId && w.Ngay == ngay, ct);
        if (entry is null)
        {
            if (r.Ml < 0) return await GetAsync(ngay, ct); // chưa có gì để trừ
            entry = new NhatKyNuocUong { NguoiDungId = userId, Ngay = ngay };
            db.NhatKyNuocUongs.Add(entry);
        }
        var total = entry.LuongMl + r.Ml;
        if (total > WaterTarget.MaxPerDayMl)
            throw ValidationAppException.Single("ml", $"Tổng lượng nước trong ngày tối đa {WaterTarget.MaxPerDayMl:N0} ml.");
        entry.LuongMl = Math.Max(0, total);
        entry.NgayCapNhat = clock.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetAsync(ngay, ct);
    }
}
