using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NutriVision.Infrastructure.Persistence;
using Xunit;

namespace NutriVision.UnitTests;

public class SqliteSchemaUpgraderTests
{
    [Fact]
    public async Task Recreates_missing_table_and_indexes_without_touching_existing_data()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(conn).Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        // Giả lập CSDL cũ: thiếu bảng NhatKyHeThong, nhưng bảng khác đã có dữ liệu.
        await db.Database.ExecuteSqlRawAsync("""INSERT INTO "ThucPhamThamChieu" ("Id","MaThucPham","TenThucPham","NhomThucPham","DonViChuan","KhoiLuongChuan","CaloTren100g","ChatDamTren100g","TinhBotTren100g","ChatBeoTren100g","ChatXoTren100g","DangHoatDong") VALUES ('11111111-1111-1111-1111-111111111111','X','Món X','Khac','g',100,100,1,1,1,1,1)""");
        await db.Database.ExecuteSqlRawAsync("""DROP TABLE "NhatKyHeThong" """);

        var created = await SqliteSchemaUpgrader.CreateMissingTablesAsync(db, NullLogger.Instance);

        Assert.Equal(["NhatKyHeThong"], created);
        Assert.Equal(1, await db.ThucPhamThamChieus.CountAsync());
        Assert.Equal(0, await db.NhatKyHeThongs.CountAsync()); // bảng dùng được
        var indexes = await db.Database.SqlQueryRaw<string>("""SELECT name AS "Value" FROM sqlite_master WHERE type='index' AND tbl_name='NhatKyHeThong'""").ToListAsync();
        Assert.NotEmpty(indexes);
        Assert.Empty(await SqliteSchemaUpgrader.CreateMissingTablesAsync(db, NullLogger.Instance)); // chạy lại: không làm gì
    }

    [Fact]
    public async Task Adds_missing_column_and_backfills_NgayCapNhat_from_NgayTao()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(conn).Options);
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("""DROP INDEX "IX_LichSuNhanDienAI_NgayCapNhat" """);
        await db.Database.ExecuteSqlRawAsync("""ALTER TABLE "LichSuNhanDienAI" DROP COLUMN "NgayCapNhat" """);
        await db.Database.ExecuteSqlRawAsync("""INSERT INTO "NguoiDung" ("Id","Email","EmailDaXacThuc","MatKhauHash","HoTen","TrangThai","VaiTro","SoLanDangNhapSai","NgayTao","NgayCapNhat") VALUES ('22222222-2222-2222-2222-222222222222','a@b.c',1,'x','A','ACTIVE','USER',0,1,1)""");
        await db.Database.ExecuteSqlRawAsync("""INSERT INTO "LichSuNhanDienAI" ("Id","NguoiDungId","DuongDanHinhAnh","NguoiDungCoSua","NgayTao") VALUES ('33333333-3333-3333-3333-333333333333','22222222-2222-2222-2222-222222222222','a.jpg',0,638000000000000000)""");

        await SqliteSchemaUpgrader.CreateMissingTablesAsync(db, NullLogger.Instance);

        var v = await db.Database.SqlQueryRaw<long>("""SELECT "NgayCapNhat" AS "Value" FROM "LichSuNhanDienAI" """).SingleAsync();
        Assert.Equal(638000000000000000, v);
    }
}
