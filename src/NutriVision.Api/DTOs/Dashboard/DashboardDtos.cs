using System;
using System.Collections.Generic;
using NutriVision.Api.DTOs.Meal;

namespace NutriVision.Api.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public decimal CaloHomNay { get; set; }
        public decimal MucTieuCalo { get; set; }
        public decimal PhanTramDatDuoc { get; set; } // ví dụ: 75.5%
        public int SoBuaAnHomNay { get; set; }
        
        // 3 nhóm chất hôm nay
        public decimal TongChatDam { get; set; }
        public decimal TongTinhBot { get; set; }
        public decimal TongChatBeo { get; set; }
        public decimal TongChatXo { get; set; }

        public List<MealDto> BuaAnsHomNay { get; set; } = new();
        public List<DailyTrendDto> XuHuong7Ngay { get; set; } = new();
    }

    public class DailyTrendDto
    {
        public string Ngay { get; set; } = string.Empty; // YYYY-MM-DD
        public string Thu { get; set; } = string.Empty; // T2, T3, ...
        public decimal CaloDaNap { get; set; }
        public decimal MucTieuCalo { get; set; }
    }

    public class ReportSummaryDto
    {
        public string TuNgay { get; set; } = string.Empty;
        public string DenNgay { get; set; } = string.Empty;
        public int TongSoNgay { get; set; }
        public int TongSoBuaAn { get; set; }
        
        public decimal CaloTrungBinhNgay { get; set; }
        public decimal ChatDamTrungBinhNgay { get; set; }
        public decimal TinhBotTrungBinhNgay { get; set; }
        public decimal ChatBeoTrungBinhNgay { get; set; }
        public decimal ChatXoTrungBinhNgay { get; set; }

        public decimal TiLePhanBoDam { get; set; } // % calo từ đạm
        public decimal TiLePhanBoTinhBot { get; set; } // % calo từ carbs
        public decimal TiLePhanBoBeo { get; set; } // % calo từ fat

        public List<DailyTrendDto> ChiTietCacNgay { get; set; } = new();
        public List<string> NhanXetThongMinh { get; set; } = new();
    }
}
