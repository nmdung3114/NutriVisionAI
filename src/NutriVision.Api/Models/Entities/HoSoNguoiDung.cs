using System;

namespace NutriVision.Api.Models.Entities
{
    public class HoSoNguoiDung
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid NguoiDungId { get; set; }
        public DateOnly NgaySinh { get; set; }
        public string GioiTinh { get; set; } = "NAM"; // NAM, NU
        public decimal ChieuCao { get; set; } // cm (50 - 200)
        public decimal CanNang { get; set; } // kg (10 - 150)
        public string MucDoVanDong { get; set; } = "VAN_DONG_VUA"; // IT_VAN_DONG, VAN_DONG_NHE, VAN_DONG_VUA, VAN_DONG_NHIEU, VAN_DONG_RAT_NHIEU
        
        // Các chỉ số tính tự động theo công thức khoa học Mifflin-St Jeor
        public decimal ChiSoBMI { get; set; }
        public decimal ChiSoBMR { get; set; }
        public decimal ChiSoTDEE { get; set; }
        public decimal MucTieuCalo { get; set; }
        
        public string? DuongDanAnhDaiDien { get; set; }
        public DateTimeOffset NgayCapNhat { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public virtual NguoiDung NguoiDung { get; set; } = null!;
    }
}
