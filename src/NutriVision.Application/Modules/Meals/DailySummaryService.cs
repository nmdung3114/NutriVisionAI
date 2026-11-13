using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Application.Modules.Meals;

public interface IDailySummaryService
{
    /// <summary>Tính lại tổng hợp ngày từ các bữa đã xác nhận & chưa xóa (BR-26, BR-28). Lưu vào DB.</summary>
    Task RecomputeAsync(Guid userId, DateOnly ngay, CancellationToken ct = default);
}

public class DailySummaryService(IApplicationDbContext db, AppClock clock) : IDailySummaryService
{
    public async Task RecomputeAsync(Guid userId, DateOnly ngay, CancellationToken ct = default)
    {
        var meals = await db.BuaAns.AsNoTracking()
            .Where(b => b.NguoiDungId == userId && b.NgayAn == ngay && b.DaXacNhan && !b.DaXoa)
            .Select(b => new { b.TongCalo, b.TongChatDam, b.TongTinhBot, b.TongChatBeo, b.TongChatXo })
            .ToListAsync(ct);

        var row = await db.TongHopDinhDuongNgays.FirstOrDefaultAsync(t => t.NguoiDungId == userId && t.Ngay == ngay, ct);

        if (meals.Count == 0)
        {
            if (row is not null) db.TongHopDinhDuongNgays.Remove(row);
            await db.SaveChangesAsync(ct);
            return;
        }

        var total = NutritionValues.Zero;
        foreach (var m in meals) total += new NutritionValues(m.TongCalo, m.TongChatDam, m.TongTinhBot, m.TongChatBeo, m.TongChatXo);

        var target = await db.HoSoNguoiDungs.AsNoTracking().Where(h => h.NguoiDungId == userId)
            .Select(h => (decimal?)h.MucTieuCalo).FirstOrDefaultAsync(ct);
        var mucTieu = target is > 0 ? target.Value : HealthMetricsCalculator.DefaultCalorieTarget;

        if (row is null)
        {
            row = new TongHopDinhDuongNgay { NguoiDungId = userId, Ngay = ngay };
            db.TongHopDinhDuongNgays.Add(row);
        }
        row.Gan(total, meals.Count, mucTieu, clock.UtcNow);
        await db.SaveChangesAsync(ct);
    }
}
