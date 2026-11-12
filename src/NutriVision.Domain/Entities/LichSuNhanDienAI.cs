using NutriVision.Domain.Common;

namespace NutriVision.Domain.Entities;

/// <summary>Lịch sử nhận diện AI: lưu phản hồi gốc để hoàn tác và đánh giá độ chính xác (BR-31).</summary>
public class LichSuNhanDienAI : BaseEntity
{
    public Guid NguoiDungId { get; set; }
    public Guid? BuaAnId { get; set; }
    public string DuongDanHinhAnh { get; set; } = string.Empty;
    /// <summary>JSON chuẩn hóa kết quả AI gốc (tên món, độ tin cậy, thành phần).</summary>
    public string? PhanHoiAIGoc { get; set; }
    public string? TenMonAIDoan { get; set; }
    public string? TenMonNguoiDungChot { get; set; }
    public decimal? DoTinCay { get; set; }
    public bool NguoiDungCoSua { get; set; }
    public int? ThoiGianXuLyMs { get; set; }
    public DateTimeOffset NgayTao { get; set; }

    public NguoiDung? NguoiDung { get; set; }
    public BuaAn? BuaAn { get; set; }
}
