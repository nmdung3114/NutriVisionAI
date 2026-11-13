using NutriVision.Domain.Common;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Domain.Entities;

/// <summary>Thành phần của bữa ăn (bảng ThanhPhanBuaAn) kèm giá trị dinh dưỡng đã quy đổi.</summary>
public class ThanhPhanBuaAn : BaseEntity
{
    public Guid BuaAnId { get; set; }
    public Guid? ThucPhamThamChieuId { get; set; }
    public string TenThanhPhan { get; set; } = string.Empty;
    public decimal KhauPhan { get; set; }
    public string DonViTinh { get; set; } = "g";
    public decimal KhoiLuongQuyDoiGram { get; set; }
    public decimal LuongCalo { get; set; }
    public decimal LuongChatDam { get; set; }
    public decimal LuongTinhBot { get; set; }
    public decimal LuongChatBeo { get; set; }
    public decimal LuongChatXo { get; set; }

    public BuaAn? BuaAn { get; set; }
    public ThucPhamThamChieu? ThucPhamThamChieu { get; set; }

    public NutritionValues DinhDuong => new(LuongCalo, LuongChatDam, LuongTinhBot, LuongChatBeo, LuongChatXo);

    public void GanDinhDuong(NutritionValues v)
    {
        var r = v.Round();
        LuongCalo = r.Calo;
        LuongChatDam = r.ChatDam;
        LuongTinhBot = r.TinhBot;
        LuongChatBeo = r.ChatBeo;
        LuongChatXo = r.ChatXo;
    }
}
