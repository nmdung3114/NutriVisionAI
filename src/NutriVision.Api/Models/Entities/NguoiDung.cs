using System;
using System.Collections.Generic;

namespace NutriVision.Api.Models.Entities
{
    public class NguoiDung : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public bool EmailDaXacThuc { get; set; } = false;
        public string MatKhauHash { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "PENDING"; // PENDING, ACTIVE, LOCKED
        public string VaiTro { get; set; } = "USER"; // USER, ADMIN
        public int SoLanDangNhapSai { get; set; } = 0;
        public DateTimeOffset? ThoiGianHetHanKhoa { get; set; }
        
        // Token xác thực email & quên mật khẩu
        public string? TokenXacThucEmail { get; set; }
        public DateTimeOffset? ThoiGianHetHanXacThuc { get; set; }
        public string? TokenDatLaiMatKhau { get; set; }
        public DateTimeOffset? ThoiGianHetHanDatLai { get; set; }

        // Navigation properties
        public virtual HoSoNguoiDung? HoSo { get; set; }
        public virtual ICollection<TokenLamMoi> Tokens { get; set; } = new List<TokenLamMoi>();
        public virtual ICollection<BuaAn> BuaAns { get; set; } = new List<BuaAn>();
        public virtual ICollection<TongHopDinhDuongNgay> TongHopDinhDuongs { get; set; } = new List<TongHopDinhDuongNgay>();
        public virtual ICollection<LichSuNhanDienAI> LichSuNhanDiens { get; set; } = new List<LichSuNhanDienAI>();
        public virtual ICollection<NhatKyHeThong> NhatKyHeThongs { get; set; } = new List<NhatKyHeThong>();
    }
}
