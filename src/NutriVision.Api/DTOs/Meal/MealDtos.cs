using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NutriVision.Api.DTOs.Meal
{
    public class ScanMealResponse
    {
        public Guid BuaAnId { get; set; }
        public string TenMonAn { get; set; } = string.Empty;
        public string DuongDanHinhAnh { get; set; } = string.Empty;
        public decimal? DoTinCay { get; set; } // ví dụ 0.95 (95%)
        public bool CanManualInput { get; set; } = false;
        public string ThongBao { get; set; } = string.Empty;
        
        public List<MealComponentDto> ThanhPhans { get; set; } = new();
        public NutritionSummaryDto TongDinhDuongTamTinh { get; set; } = new();
    }

    public class MealComponentDto
    {
        public Guid Id { get; set; }
        public Guid? ThucPhamThamChieuId { get; set; }
        public string TenThanhPhan { get; set; } = string.Empty;
        public decimal KhauPhan { get; set; } = 1;
        public string DonViTinh { get; set; } = "phần";
        public decimal KhoiLuongQuyDoiGram { get; set; } = 100;
        
        public decimal LuongCalo { get; set; }
        public decimal LuongChatDam { get; set; }
        public decimal LuongTinhBot { get; set; }
        public decimal LuongChatBeo { get; set; }
        public decimal LuongChatXo { get; set; }
    }

    public class NutritionSummaryDto
    {
        public decimal TongCalo { get; set; }
        public decimal TongChatDam { get; set; }
        public decimal TongTinhBot { get; set; }
        public decimal TongChatBeo { get; set; }
        public decimal TongChatXo { get; set; }
    }

    public class MealDto
    {
        public Guid Id { get; set; }
        public string LoaiBuaAn { get; set; } = "SANG"; // SANG, TRUA, TOI, PHU
        public DateOnly NgayAn { get; set; }
        public DateTimeOffset ThoiGianGhiNhan { get; set; }
        public string TenMonAn { get; set; } = string.Empty;
        public string? DuongDanHinhAnh { get; set; }
        
        public decimal TongCalo { get; set; }
        public decimal TongChatDam { get; set; }
        public decimal TongTinhBot { get; set; }
        public decimal TongChatBeo { get; set; }
        public decimal TongChatXo { get; set; }

        public bool TaoTuAI { get; set; }
        public bool DaXacNhan { get; set; }
        public List<MealComponentDto> ThanhPhans { get; set; } = new();
    }

    public class AdjustMealRequest
    {
        [Required(ErrorMessage = "Tên món ăn không được để trống")]
        public string TenMonAn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bữa ăn phải có ít nhất 1 thành phần")]
        [MinLength(1, ErrorMessage = "Bữa ăn phải có ít nhất 1 thành phần")]
        public List<AdjustMealComponentDto> ThanhPhans { get; set; } = new();
    }

    public class AdjustMealComponentDto
    {
        public Guid? Id { get; set; }
        public Guid? ThucPhamThamChieuId { get; set; }

        [Required(ErrorMessage = "Tên thành phần là bắt buộc")]
        public string TenThanhPhan { get; set; } = string.Empty;

        [Range(0.01, 10000, ErrorMessage = "Khẩu phần phải lớn hơn 0")]
        public decimal KhauPhan { get; set; } = 1;

        public string DonViTinh { get; set; } = "phần";

        [Range(1, 50000, ErrorMessage = "Khối lượng quy đổi phải lớn hơn 0")]
        public decimal KhoiLuongQuyDoiGram { get; set; } = 100;
    }

    public class ManualMealRequest
    {
        [Required]
        public string LoaiBuaAn { get; set; } = "SANG";

        [Required]
        public string TenMonAn { get; set; } = string.Empty;

        public DateOnly? NgayAn { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Bữa ăn phải có ít nhất 1 thành phần")]
        public List<AdjustMealComponentDto> ThanhPhans { get; set; } = new();
    }
}
