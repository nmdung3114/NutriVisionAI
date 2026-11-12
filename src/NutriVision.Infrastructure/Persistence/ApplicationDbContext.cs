using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Domain.Entities;

namespace NutriVision.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<HoSoNguoiDung> HoSoNguoiDungs => Set<HoSoNguoiDung>();
    public DbSet<TokenLamMoi> TokenLamMois => Set<TokenLamMoi>();
    public DbSet<ThucPhamThamChieu> ThucPhamThamChieus => Set<ThucPhamThamChieu>();
    public DbSet<BuaAn> BuaAns => Set<BuaAn>();
    public DbSet<ThanhPhanBuaAn> ThanhPhanBuaAns => Set<ThanhPhanBuaAn>();
    public DbSet<TongHopDinhDuongNgay> TongHopDinhDuongNgays => Set<TongHopDinhDuongNgay>();
    public DbSet<LichSuNhanDienAI> LichSuNhanDienAIs => Set<LichSuNhanDienAI>();
    public DbSet<NhatKyHeThong> NhatKyHeThongs => Set<NhatKyHeThong>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // BR-35: xóa mềm – mọi truy vấn mặc định bỏ qua bữa đã xóa.
        modelBuilder.Entity<BuaAn>().HasQueryFilter(b => !b.DaXoa);
        modelBuilder.Entity<ThanhPhanBuaAn>().HasQueryFilter(t => !t.BuaAn!.DaXoa);
        modelBuilder.Entity<LichSuNhanDienAI>().HasQueryFilter(l => l.BuaAn == null || !l.BuaAn.DaXoa);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite không so sánh/sắp xếp được DateTimeOffset: lưu dưới dạng ticks UTC.
        if (Database.IsSqlite())
        {
            configurationBuilder.Properties<DateTimeOffset>().HaveConversion<SqliteDateTimeOffsetConverter>();
            configurationBuilder.Properties<DateTimeOffset?>().HaveConversion<SqliteNullableDateTimeOffsetConverter>();
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) { await action(); return; }
        await using var tx = await Database.BeginTransactionAsync(cancellationToken);
        await action();
        await tx.CommitAsync(cancellationToken);
    }
}
