using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NutriVision.Application.Abstractions;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;

namespace NutriVision.Application.Modules.Foods;

public interface IFoodCatalog
{
    /// <summary>Toàn bộ thực phẩm đang hoạt động (cache ngắn hạn, dữ liệu tham chiếu ít thay đổi).</summary>
    Task<IReadOnlyList<ThucPhamThamChieu>> GetActiveAsync(CancellationToken ct = default);
}

public class FoodCatalog(IApplicationDbContext db, IMemoryCache cache) : IFoodCatalog
{
    private const string Key = "foods:active";

    public async Task<IReadOnlyList<ThucPhamThamChieu>> GetActiveAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(Key, out IReadOnlyList<ThucPhamThamChieu>? cached) && cached is not null) return cached;
        var list = await db.ThucPhamThamChieus.AsNoTracking().Where(f => f.DangHoatDong).OrderBy(f => f.TenThucPham).ToListAsync(ct);
        cache.Set(Key, (IReadOnlyList<ThucPhamThamChieu>)list, TimeSpan.FromMinutes(10));
        return list;
    }
}

public interface IFoodService
{
    Task<IReadOnlyList<FoodDto>> SearchAsync(string? q, string? nhom, int take, CancellationToken ct = default);
    Task<IReadOnlyList<FoodMatchDto>> MatchAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GroupsAsync(CancellationToken ct = default);
    IReadOnlyList<UnitDto> Units();
}

public class FoodService(IFoodCatalog catalog) : IFoodService
{
    public async Task<IReadOnlyList<FoodDto>> SearchAsync(string? q, string? nhom, int take, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 100);
        var foods = await catalog.GetActiveAsync(ct);
        IEnumerable<ThucPhamThamChieu> query = foods;
        if (!string.IsNullOrWhiteSpace(nhom))
        {
            var g = TextNormalizer.Chuan(nhom);
            query = query.Where(f => TextNormalizer.Chuan(f.NhomThucPham) == g);
        }

        if (string.IsNullOrWhiteSpace(q))
            return query.Take(take).Select(FoodDto.From).ToList();

        var nq = TextNormalizer.Chuan(q);
        return query
            .Select(f => (Food: f, Norm: TextNormalizer.Chuan(f.TenThucPham)))
            .Where(x => x.Norm.Contains(nq) || nq.Split(' ').All(t => x.Norm.Contains(t)))
            .OrderBy(x => x.Norm.StartsWith(nq) ? 0 : x.Norm.Contains(" " + nq) ? 1 : 2)
            .ThenBy(x => x.Norm.Length)
            .Take(take)
            .Select(x => FoodDto.From(x.Food))
            .ToList();
    }

    public async Task<IReadOnlyList<FoodMatchDto>> MatchAsync(string name, CancellationToken ct = default)
    {
        var foods = await catalog.GetActiveAsync(ct);
        return FoodMatcher.Match(name, foods, 5, 0.3)
            .Select(m => new FoodMatchDto(FoodDto.From(m.Food), (decimal)Math.Round(m.Score, 3)))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GroupsAsync(CancellationToken ct = default) =>
        (await catalog.GetActiveAsync(ct)).Select(f => f.NhomThucPham).Distinct().OrderBy(x => x).ToList();

    public IReadOnlyList<UnitDto> Units() =>
        NutritionCalculator.DefaultUnitGrams
            .Select(kv => new UnitDto(NutritionCalculator.UnitLabels.GetValueOrDefault(kv.Key, kv.Key), kv.Value))
            .ToList();
}
