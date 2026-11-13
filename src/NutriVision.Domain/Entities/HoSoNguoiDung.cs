using NutriVision.Domain.Common;

namespace NutriVision.Domain.Entities;

/// <summary>Hồ sơ cá nhân và các chỉ số cơ thể (bảng HoSoNguoiDung). Quan hệ 1-1 với NguoiDung.</summary>
public class HoSoNguoiDung : BaseEntity
{
    public Guid NguoiDungId { get; set; }
    public DateOnly NgaySinh { get; set; }
    public string GioiTinh { get; set; } = string.Empty;
    public decimal ChieuCao { get; set; }
    public decimal CanNang { get; set; }
    public string MucDoVanDong { get; set; } = string.Empty;
    public decimal ChiSoBMI { get; set; }
    public decimal ChiSoBMR { get; set; }
    public decimal ChiSoTDEE { get; set; }
    public decimal MucTieuCalo { get; set; }
    public string? DuongDanAnhDaiDien { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }

    public NguoiDung? NguoiDung { get; set; }
}
