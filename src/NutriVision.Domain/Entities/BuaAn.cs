using NutriVision.Domain.Common;
using NutriVision.Domain.Constants;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Domain.Entities;

/// <summary>
/// Bữa ăn (bảng BuaAn). Trạng thái: bản nháp (DaXacNhan=false) → đã xác nhận (DaXacNhan=true) → đã xóa mềm (DaXoa=true).
/// Chỉ bữa đã xác nhận và chưa xóa mới được tính vào tổng hợp dinh dưỡng (BR-26, BR-28).
/// </summary>
public class BuaAn : BaseEntity
{
    public Guid NguoiDungId { get; set; }
    public string LoaiBuaAn { get; set; } = Constants.LoaiBuaAn.Sang;
    public DateOnly NgayAn { get; set; }
    public DateTimeOffset ThoiGianGhiNhan { get; set; }
    public string TenMonAn { get; set; } = string.Empty;
    public string? DuongDanHinhAnh { get; set; }
    public decimal TongCalo { get; private set; }
    public decimal TongChatDam { get; private set; }
    public decimal TongTinhBot { get; private set; }
    public decimal TongChatBeo { get; private set; }
    public decimal TongChatXo { get; private set; }
    public bool TaoTuAI { get; set; }
    public bool DaXacNhan { get; private set; }
    public bool DaXoa { get; private set; }
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }

    public NguoiDung? NguoiDung { get; set; }
    public ICollection<ThanhPhanBuaAn> ThanhPhans { get; set; } = new List<ThanhPhanBuaAn>();
    public LichSuNhanDienAI? LichSuNhanDien { get; set; }

    public NutritionValues TongDinhDuong => new(TongCalo, TongChatDam, TongTinhBot, TongChatBeo, TongChatXo);

    public bool TinhVaoTongHop => DaXacNhan && !DaXoa;

    /// <summary>Gán tổng dinh dưỡng = tổng các thành phần (AC-36).</summary>
    public void GanTongDinhDuong(NutritionValues total, DateTimeOffset now)
    {
        var t = total.Round();
        TongCalo = t.Calo;
        TongChatDam = t.ChatDam;
        TongTinhBot = t.TinhBot;
        TongChatBeo = t.ChatBeo;
        TongChatXo = t.ChatXo;
        NgayCapNhat = now;
    }

    public void XacNhan(DateTimeOffset now)
    {
        DaXacNhan = true;
        NgayCapNhat = now;
    }

    /// <summary>Xóa mềm (BR-35).</summary>
    public void XoaMem(DateTimeOffset now)
    {
        DaXoa = true;
        NgayCapNhat = now;
    }
}
