using System.Text.Json;
using NutriVision.Application.Modules.Foods;
using NutriVision.Domain.Entities;
using NutriVision.Domain.Services;
using NutriVision.Application.Abstractions;

namespace NutriVision.Application.Modules.Meals;

public static class MealJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    public static string Serialize(AiOriginal o) => JsonSerializer.Serialize(o, Options);
    public static AiOriginal? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<AiOriginal>(json, Options); }
        catch (JsonException) { return null; }
    }
}

/// <summary>Ánh xạ entity → DTO.</summary>
public class MealMapper(IFileUrlSigner signer)
{
    public MealDto ToDto(BuaAn meal, decimal mucTieuCalo, bool aiThatBai = false, string? thongBaoAI = null)
    {
        var lichSu = meal.LichSuNhanDien;
        var aiGoc = MealJson.Deserialize(lichSu?.PhanHoiAIGoc);
        var tong = meal.TongDinhDuong;
        var phanTram = mucTieuCalo > 0 ? Math.Round(tong.Calo / mucTieuCalo * 100m, 1) : 0m;

        return new MealDto(
            meal.Id, meal.LoaiBuaAn, meal.NgayAn, meal.ThoiGianGhiNhan, meal.TenMonAn, signer.Sign(meal.DuongDanHinhAnh),
            meal.TaoTuAI, meal.DaXacNhan,
            meal.ThanhPhans.OrderByDescending(t => t.LuongCalo).ThenBy(t => t.TenThanhPhan).Select(t => ToComponentDto(t, meal.DaXacNhan)).ToList(),
            NutritionDto.From(tong), mucTieuCalo, phanTram,
            lichSu?.DoTinCay, aiGoc, lichSu is null ? null : lichSu.NguoiDungCoSua,
            aiThatBai, thongBaoAI);
    }

    public string? SignPath(string? path) => signer.Sign(path);

    public MealSummaryDto ToSummary(BuaAn meal) => new(
        meal.Id, meal.LoaiBuaAn, meal.NgayAn, meal.ThoiGianGhiNhan, meal.TenMonAn, signer.Sign(meal.DuongDanHinhAnh),
        meal.TaoTuAI, NutritionDto.From(meal.TongDinhDuong), meal.ThanhPhans.Count);

    private static MealComponentDto ToComponentDto(ThanhPhanBuaAn t, bool daXacNhan)
    {
        FoodDto? food = t.ThucPhamThamChieu is null ? null : FoodDto.From(t.ThucPhamThamChieu);
        NutritionDto? per100 = null;
        if (t.ThucPhamThamChieu is not null)
            per100 = NutritionDto.From(t.ThucPhamThamChieu.DinhDuongTren100g);
        else if (t.KhoiLuongQuyDoiGram > 0 && !t.DinhDuong.IsEmpty)
            per100 = NutritionDto.From((t.DinhDuong * (100m / t.KhoiLuongQuyDoiGram)).Round());

        var thieu = !daXacNhan && t.ThucPhamThamChieuId is null && t.DinhDuong.IsEmpty;
        return new MealComponentDto(t.Id, t.ThucPhamThamChieuId, t.TenThanhPhan, t.KhauPhan, t.DonViTinh, t.KhoiLuongQuyDoiGram,
            t.LuongCalo, t.LuongChatDam, t.LuongTinhBot, t.LuongChatBeo, t.LuongChatXo, food, per100, thieu);
    }

    /// <summary>Kết quả sau hiệu chỉnh có khác kết quả AI gốc không (NguoiDungCoSua).</summary>
    public static bool IsModified(AiOriginal original, string tenMon, IReadOnlyList<ComputedComponent> finalComponents)
    {
        if (TextNormalizer.Chuan(original.TenMonAn) != TextNormalizer.Chuan(tenMon)) return true;
        if (original.ThanhPhan.Count != finalComponents.Count) return true;

        var remaining = finalComponents.Select(c => (Name: TextNormalizer.Chuan(c.Input.TenThanhPhan), c.Gram, FoodId: c.Food?.Id)).ToList();
        foreach (var o in original.ThanhPhan)
        {
            var key = TextNormalizer.Chuan(o.TenThanhPhan);
            var idx = remaining.FindIndex(r => r.Name == key && Math.Abs(r.Gram - o.KhoiLuongGram) <= 0.5m && (o.ThucPhamThamChieuId is null || r.FoodId == o.ThucPhamThamChieuId));
            if (idx < 0) return true;
            remaining.RemoveAt(idx);
        }
        return false;
    }
}
