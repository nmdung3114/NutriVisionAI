using NutriVision.Domain.Common;

namespace NutriVision.Domain.Entities;

/// <summary>Refresh token (bảng TokenLamMoi). MaToken lưu hash SHA-256 của token thô.</summary>
public class TokenLamMoi : BaseEntity
{
    public Guid NguoiDungId { get; set; }
    public string MaToken { get; set; } = string.Empty;
    public DateTimeOffset ThoiGianHetHan { get; set; }
    public bool DaSuDung { get; set; }
    public bool DaThuHoi { get; set; }
    public DateTimeOffset NgayTao { get; set; }

    public NguoiDung? NguoiDung { get; set; }

    public bool DangHieuLuc(DateTimeOffset now) => !DaSuDung && !DaThuHoi && ThoiGianHetHan > now;
}
