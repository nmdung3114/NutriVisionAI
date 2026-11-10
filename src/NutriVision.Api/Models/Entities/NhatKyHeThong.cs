using System;

namespace NutriVision.Api.Models.Entities
{
    public class NhatKyHeThong
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? NguoiDungThucHienId { get; set; }
        public string LoaiHanhDong { get; set; } = string.Empty; // DANG_NHAP, DANG_KY, KHOA_USER, DOI_ROLE, XOA_BUA_AN, ...
        public string? BangTacDong { get; set; }
        public string? IdDoiTuong { get; set; }
        public string? ChiTiet { get; set; }
        public string? DiaChiIP { get; set; }
        public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public virtual NguoiDung? NguoiDungThucHien { get; set; }
    }
}
