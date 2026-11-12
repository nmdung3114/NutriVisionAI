using NutriVision.Domain.Common;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Domain.Entities;

/// <summary>Danh mục thực phẩm chuẩn (bảng ThucPhamThamChieu). Giá trị dinh dưỡng tính trên 100g.</summary>
public class ThucPhamThamChieu : BaseEntity
{
    public string MaThucPham { get; set; } = string.Empty;
    public string TenThucPham { get; set; } = string.Empty;
    public string NhomThucPham { get; set; } = string.Empty;
    public string DonViChuan { get; set; } = string.Empty;
    public decimal KhoiLuongChuan { get; set; }
    public decimal CaloTren100g { get; set; }
    public decimal ChatDamTren100g { get; set; }
    public decimal TinhBotTren100g { get; set; }
    public decimal ChatBeoTren100g { get; set; }
    public decimal ChatXoTren100g { get; set; }
    public bool DangHoatDong { get; set; } = true;

    public NutritionValues DinhDuongTren100g => new(CaloTren100g, ChatDamTren100g, TinhBotTren100g, ChatBeoTren100g, ChatXoTren100g);
}
