using System;

namespace NutriVision.Api.Models.Entities
{
    public class ThanhPhanBuaAn
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BuaAnId { get; set; }
        public Guid? ThucPhamThamChieuId { get; set; }
        public string TenThanhPhan { get; set; } = string.Empty;
        
        public decimal KhauPhan { get; set; } = 1;
        public string DonViTinh { get; set; } = "phần"; // gram, ml, bát, đĩa, phần, miếng, quả
        public decimal KhoiLuongQuyDoiGram { get; set; } = 100;

        // Dinh dưỡng thành phần tương ứng
        public decimal LuongCalo { get; set; } = 0;
        public decimal LuongChatDam { get; set; } = 0;
        public decimal LuongTinhBot { get; set; } = 0;
        public decimal LuongChatBeo { get; set; } = 0;
        public decimal LuongChatXo { get; set; } = 0;

        // Navigation
        public virtual BuaAn BuaAn { get; set; } = null!;
        public virtual ThucPhamThamChieu? ThucPhamThamChieu { get; set; }
    }
}
