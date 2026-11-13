using NutriVision.Domain.Common;

namespace NutriVision.Domain.Entities;

/// <summary>Tổng lượng nước uống trong ngày (bảng NhatKyNuocUong), 1 bản ghi / người dùng / ngày.</summary>
public class NhatKyNuocUong : BaseEntity
{
    public Guid NguoiDungId { get; set; }
    public DateOnly Ngay { get; set; }
    public int LuongMl { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }

    public NguoiDung? NguoiDung { get; set; }
}
