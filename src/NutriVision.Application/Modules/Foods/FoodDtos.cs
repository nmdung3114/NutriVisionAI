using NutriVision.Domain.Entities;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Application.Modules.Foods;

public record NutritionDto(decimal Calo, decimal ChatDam, decimal TinhBot, decimal ChatBeo, decimal ChatXo)
{
    public static NutritionDto From(NutritionValues v) => new(v.Calo, v.ChatDam, v.TinhBot, v.ChatBeo, v.ChatXo);
    public NutritionValues ToValues() => new(Calo, ChatDam, TinhBot, ChatBeo, ChatXo);
}

public record FoodDto(
    Guid Id, string MaThucPham, string TenThucPham, string NhomThucPham, string DonViChuan, decimal KhoiLuongChuan,
    decimal CaloTren100g, decimal ChatDamTren100g, decimal TinhBotTren100g, decimal ChatBeoTren100g, decimal ChatXoTren100g)
{
    public static FoodDto From(ThucPhamThamChieu f) => new(
        f.Id, f.MaThucPham, f.TenThucPham, f.NhomThucPham, f.DonViChuan, f.KhoiLuongChuan,
        f.CaloTren100g, f.ChatDamTren100g, f.TinhBotTren100g, f.ChatBeoTren100g, f.ChatXoTren100g);
}

public record FoodMatchDto(FoodDto Food, decimal Diem);

public record UnitDto(string DonVi, decimal Gram);
