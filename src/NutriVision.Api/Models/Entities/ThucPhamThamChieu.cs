using System;
using System.Collections.Generic;

namespace NutriVision.Api.Models.Entities
{
    public class ThucPhamThamChieu
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string MaThucPham { get; set; } = string.Empty;
        public string TenThucPham { get; set; } = string.Empty;
        public string NhomThucPham { get; set; } = string.Empty;
        public string DonViChuan { get; set; } = "100g";
        public decimal KhoiLuongChuan { get; set; } = 100m;
        
        // Chỉ số dinh dưỡng trên 100g
        public decimal CaloTren100g { get; set; }
        public decimal ChatDamTren100g { get; set; }
        public decimal TinhBotTren100g { get; set; }
        public decimal ChatBeoTren100g { get; set; }
        public decimal ChatXoTren100g { get; set; }
        
        public bool DangHoatDong { get; set; } = true;

        // Navigation
        public virtual ICollection<ThanhPhanBuaAn> ThanhPhans { get; set; } = new List<ThanhPhanBuaAn>();
    }
}
