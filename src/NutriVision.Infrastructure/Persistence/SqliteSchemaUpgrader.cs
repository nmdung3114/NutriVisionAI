using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NutriVision.Infrastructure.Persistence;

/// <summary>
/// Chỉ dùng cho CSDL SQLite ở môi trường dev (tạo bằng EnsureCreated, không có migration):
/// khi model có thêm bảng mới, tạo các bảng + index còn thiếu mà KHÔNG xóa dữ liệu đang có.
/// Không xử lý thêm/đổi cột trong bảng cũ (khi đó xóa file App_Data/nutrivision-dev.db để tạo lại).
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
        return created;
    }
}
