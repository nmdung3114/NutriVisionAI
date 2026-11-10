using System;
using System.ComponentModel.DataAnnotations;

namespace NutriVision.Api.DTOs.Profile
{
    public class ProfileDto
    {
        public Guid NguoiDungId { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateOnly NgaySinh { get; set; }
        public int Tuoi { get; set; }
        public string GioiTinh { get; set; } = "NAM";
        public decimal ChieuCao { get; set; }
        public decimal CanNang { get; set; }
        public string MucDoVanDong { get; set; } = "VAN_DONG_VUA";
        
        // Chỉ số sức khỏe & dinh dưỡng
        public decimal ChiSoBMI { get; set; }
        public string PhanLoaiBMI { get; set; } = string.Empty; // Gầy, Bình thường, Thừa cân, Béo phì
        public decimal ChiSoBMR { get; set; }
        public decimal ChiSoTDEE { get; set; }
        public decimal MucTieuCalo { get; set; }
        public string? DuongDanAnhDaiDien { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateOnly NgaySinh { get; set; }

        [Required]
        public string GioiTinh { get; set; } = "NAM"; // NAM, NU

        [Range(50, 200, ErrorMessage = "Chiều cao phải từ 50cm đến 200cm")]
        public decimal ChieuCao { get; set; }

        [Range(10, 150, ErrorMessage = "Cân nặng phải từ 10kg đến 150kg")]
        public decimal CanNang { get; set; }

        [Required]
        public string MucDoVanDong { get; set; } = "VAN_DONG_VUA";
    }
}
