using NutriVision.Domain.Common;

namespace NutriVision.Domain.Entities;

/// <summary>Nhật ký cân nặng theo ngày (bảng NhatKyCanNang), tối đa 1 bản ghi / người dùng / ngày.</summary>
public class NhatKyCanNang : BaseEntity
{
    public Guid NguoiDungId { get; set; }
    public DateOnly Ngay { get; set; }
    public decimal CanNang { get; set; }
    public string? GhiChu { get; set; }
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }

    public NguoiDung? NguoiDung { get; set; }
}
