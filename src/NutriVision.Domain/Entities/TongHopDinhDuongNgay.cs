using NutriVision.Domain.Common;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Domain.Entities;

/// <summary>Tổng hợp dinh dưỡng theo ngày của từng người dùng (bảng TongHopDinhDuongNgay), chỉ gồm bữa đã xác nhận.</summary>
public class TongHopDinhDuongNgay : BaseEntity
{
    public Guid NguoiDungId { get; set; }
    public DateOnly Ngay { get; set; }
    public decimal TongCalo { get; set; }
    public decimal TongChatDam { get; set; }
    public decimal TongTinhBot { get; set; }
    public decimal TongChatBeo { get; set; }
    public decimal TongChatXo { get; set; }
    public int SoBuaAnDaGhi { get; set; }
    public decimal MucTieuCaloNgay { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }

    public NutritionValues Tong => new(TongCalo, TongChatDam, TongTinhBot, TongChatBeo, TongChatXo);

    public void Gan(NutritionValues total, int soBua, decimal mucTieu, DateTimeOffset now)
    {
        var t = total.Round();
        TongCalo = t.Calo;
        TongChatDam = t.ChatDam;
        TongTinhBot = t.TinhBot;
        TongChatBeo = t.ChatBeo;
        TongChatXo = t.ChatXo;
        SoBuaAnDaGhi = soBua;
        MucTieuCaloNgay = mucTieu;
        NgayCapNhat = now;
    }
}
