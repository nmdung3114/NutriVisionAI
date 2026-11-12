using Microsoft.EntityFrameworkCore;
using NutriVision.Domain.Entities;

namespace NutriVision.Application.Abstractions;

/// <summary>Cổng truy cập dữ liệu mà tầng Application phụ thuộc; được hiện thực ở Infrastructure bằng EF Core.</summary>
public interface IApplicationDbContext
{
    DbSet<NguoiDung> NguoiDungs { get; }
    DbSet<HoSoNguoiDung> HoSoNguoiDungs { get; }
    DbSet<TokenLamMoi> TokenLamMois { get; }
    DbSet<ThucPhamThamChieu> ThucPhamThamChieus { get; }
    DbSet<BuaAn> BuaAns { get; }
    DbSet<ThanhPhanBuaAn> ThanhPhanBuaAns { get; }
    DbSet<TongHopDinhDuongNgay> TongHopDinhDuongNgays { get; }
    DbSet<LichSuNhanDienAI> LichSuNhanDienAIs { get; }
    DbSet<NhatKyHeThong> NhatKyHeThongs { get; }
    DbSet<NhatKyCanNang> NhatKyCanNangs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Chạy nhiều thao tác lưu trong một transaction.</summary>
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
