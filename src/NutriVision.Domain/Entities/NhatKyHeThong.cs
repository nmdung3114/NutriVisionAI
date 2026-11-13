using NutriVision.Domain.Common;

namespace NutriVision.Domain.Entities;

/// <summary>Nhật ký hệ thống / thao tác quản trị (bảng NhatKyHeThong).</summary>
public class NhatKyHeThong : BaseEntity
{
    public Guid? NguoiDungThucHienId { get; set; }
    public string LoaiHanhDong { get; set; } = string.Empty;
    public string? BangTacDong { get; set; }
    public string? IdDoiTuong { get; set; }
    public string? ChiTiet { get; set; }
    public string? DiaChiIP { get; set; }
    public DateTimeOffset NgayTao { get; set; }

    public NguoiDung? NguoiDungThucHien { get; set; }
}
