using System;

namespace NutriVision.Api.Models.Entities
{
    public class LichSuNhanDienAI
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid NguoiDungId { get; set; }
        public Guid? BuaAnId { get; set; }
        public string DuongDanHinhAnh { get; set; } = string.Empty;
        
        // Phản hồi JSON gốc từ mô hình AI
        public string? PhanHoiAIGoc { get; set; }
        public string? TenMonAIDoan { get; set; }
        public string? TenMonNguoiDungChot { get; set; }
        public decimal? DoTinCay { get; set; } // ví dụ: 0.950 (95%)
        public bool NguoiDungCoSua { get; set; } = false; // Đánh giá AI drift/accuracy
        public int? ThoiGianXuLyMs { get; set; }
        
        public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public virtual NguoiDung NguoiDung { get; set; } = null!;
        public virtual BuaAn? BuaAn { get; set; }
    }
}
