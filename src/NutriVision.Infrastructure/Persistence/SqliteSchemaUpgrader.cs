using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace NutriVision.Infrastructure.Persistence;

/// <summary>
/// Chỉ dùng cho CSDL SQLite ở môi trường dev (tạo bằng EnsureCreated, không có migration):
/// khi model có thêm bảng mới, tạo các bảng + index còn thiếu mà KHÔNG xóa dữ liệu đang có.
/// Thêm được cột mới vào bảng cũ (giá trị mặc định 0/rỗng; cột NgayCapNhat mới lấy theo NgayTao).
/// Không xử lý đổi kiểu/xóa cột (khi đó xóa file App_Data/nutrivision-dev.db để tạo lại).
/// Môi trường SQL Server dùng migration EF Core như bình thường.
/// </summary>
public static partial class SqliteSchemaUpgrader
{
    [GeneratedRegex("""^CREATE TABLE "(?<t>[^"]+)""", RegexOptions.Multiline)]
    private static partial Regex CreateTable();

    [GeneratedRegex("""^CREATE (UNIQUE )?INDEX "[^"]+" ON "(?<t>[^"]+)""", RegexOptions.Multiline)]
    private static partial Regex CreateIndex();

    public static async Task<IReadOnlyList<string>> CreateMissingTablesAsync(DbContext db, ILogger logger, CancellationToken ct = default)
    {
        if (!db.Database.IsSqlite()) return [];

        var existing = await db.Database
            .SqlQueryRaw<string>("SELECT name AS \"Value\" FROM sqlite_master WHERE type = 'table'")
            .ToListAsync(ct);
        var have = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var statements = db.Database.GenerateCreateScript()
            .Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length > 0)
            .ToList();

        var created = new List<string>();
        foreach (var sql in statements)
        {
            var t = CreateTable().Match(sql);
            if (t.Success && !have.Contains(t.Groups["t"].Value))
            {
                await db.Database.ExecuteSqlRawAsync(sql, ct);
                created.Add(t.Groups["t"].Value);
            }
        }
        foreach (var sql in statements)
        {
            var i = CreateIndex().Match(sql);
            if (i.Success && created.Contains(i.Groups["t"].Value))
                await db.Database.ExecuteSqlRawAsync(sql, ct);
        }

        if (created.Count > 0) logger.LogInformation("SQLite dev: đã tạo bảng mới {Tables}", string.Join(", ", created));
        await AddMissingColumnsAsync(db, created, logger, ct);
        return created;
    }

    private static async Task AddMissingColumnsAsync(DbContext db, IReadOnlyCollection<string> justCreated, ILogger logger, CancellationToken ct)
    {
        foreach (var et in db.Model.GetEntityTypes())
        {
            var table = et.GetTableName();
            if (table is null || justCreated.Contains(table)) continue;
            var store = StoreObjectIdentifier.Table(table, et.GetSchema());
            var existing = (await db.Database.SqlQueryRaw<string>($"SELECT name AS \"Value\" FROM pragma_table_info('{table}')").ToListAsync(ct))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (existing.Count == 0) continue;

            foreach (var p in et.GetProperties())
            {
                var col = p.GetColumnName(store);
                if (col is null || existing.Contains(col)) continue;
                var type = p.GetColumnType();
                var dflt = p.IsColumnNullable(store) ? "" : type.Contains("TEXT", StringComparison.OrdinalIgnoreCase) ? " NOT NULL DEFAULT ''" : " NOT NULL DEFAULT 0";
                await db.Database.ExecuteSqlRawAsync($"ALTER TABLE \"{table}\" ADD COLUMN \"{col}\" {type}{dflt}", ct);
                if (col == "NgayCapNhat" && existing.Contains("NgayTao"))
                    await db.Database.ExecuteSqlRawAsync($"UPDATE \"{table}\" SET \"NgayCapNhat\" = \"NgayTao\"", ct);
                logger.LogInformation("SQLite dev: đã thêm cột {Table}.{Column}", table, col);
            }
        }
    }
}
