using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;
using NutriVision.Infrastructure.Persistence;
using NutriVision.Infrastructure.Settings;

namespace NutriVision.Infrastructure.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        if (settings.AutoMigrate)
        {
            if (db.Database.IsSqlite()) await db.Database.EnsureCreatedAsync(ct);
            else await db.Database.MigrateAsync(ct);
        }

        if (settings.SeedFoods) await SeedFoodsAsync(db, logger, ct);
        if (settings.SeedDemoAccounts) await SeedDemoAccountsAsync(db, sp.GetRequiredService<IPasswordHasher>(), sp.GetRequiredService<AppClock>(), logger, ct);
    }

    private static async Task SeedFoodsAsync(ApplicationDbContext db, ILogger logger, CancellationToken ct)
    {
        var existing = await db.ThucPhamThamChieus.Select(f => f.MaThucPham).ToListAsync(ct);
        var set = existing.ToHashSet();
        var added = 0;
        foreach (var i in FoodSeedData.Items.Where(i => !set.Contains(i.Ma)))
        {
            db.ThucPhamThamChieus.Add(new ThucPhamThamChieu
            {
                MaThucPham = i.Ma, TenThucPham = i.Ten, NhomThucPham = i.Nhom, DonViChuan = i.DonVi, KhoiLuongChuan = i.KhoiLuong,
                CaloTren100g = i.Calo, ChatDamTren100g = i.Dam, TinhBotTren100g = i.TinhBot, ChatBeoTren100g = i.Beo, ChatXoTren100g = i.Xo,
                DangHoatDong = true,
            });
            added++;
        }
        if (added > 0) { await db.SaveChangesAsync(ct); logger.LogInformation("Đã nạp {Count} thực phẩm tham chiếu", added); }
    }

    /// <summary>Tài khoản demo (chỉ bật ở môi trường phát triển).</summary>
    private static async Task SeedDemoAccountsAsync(ApplicationDbContext db, IPasswordHasher hasher, AppClock clock, ILogger logger, CancellationToken ct)
    {
        var now = clock.UtcNow;
        if (!await db.NguoiDungs.AnyAsync(u => u.Email == "admin@nutrivision.ai", ct))
            db.NguoiDungs.Add(NguoiDung.TaoDaKichHoat("admin@nutrivision.ai", "Trần Quản Trị", hasher.Hash("Admin@123"), VaiTro.Admin, now));

        if (!await db.NguoiDungs.AnyAsync(u => u.Email == "user@nutrivision.ai", ct))
        {
            var user = NguoiDung.TaoDaKichHoat("user@nutrivision.ai", "Nguyễn Minh Anh", hasher.Hash("User@123"), VaiTro.User, now);
            var dob = new DateOnly(2000, 1, 1);
            var m = HealthMetricsCalculator.Tinh(GioiTinh.Nam, 172m, 68m, HealthMetricsCalculator.TinhTuoi(dob, clock.Today), MucDoVanDong.VuaPhai);
            user.HoSo = new HoSoNguoiDung
            {
                NgaySinh = dob, GioiTinh = GioiTinh.Nam, ChieuCao = 172m, CanNang = 68m, MucDoVanDong = MucDoVanDong.VuaPhai,
                ChiSoBMI = m.Bmi, ChiSoBMR = m.Bmr, ChiSoTDEE = m.Tdee, MucTieuCalo = Math.Round(m.Tdee), NgayCapNhat = now,
            };
            db.NguoiDungs.Add(user);
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Đã nạp tài khoản demo");
    }
}
