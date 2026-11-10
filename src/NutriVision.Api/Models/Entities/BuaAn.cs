using System;
using System.Collections.Generic;

namespace NutriVision.Api.Models.Entities
{
    public class BuaAn : BaseEntity
    {
        public Guid NguoiDungId { get; set; }
        public string LoaiBuaAn { get; set; } = "SANG"; // SANG, TRUA, TOI, PHU
        public DateOnly NgayAn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public DateTimeOffset ThoiGianGhiNhan { get; set; } = DateTimeOffset.UtcNow;
        public string TenMonAn { get; set; } = string.Empty;
        public string? DuongDanHinhAnh { get; set; }
        
        // Tổng các giá trị dinh dưỡng của toàn bộ bữa ăn
        public decimal TongCalo { get; set; } = 0;
        public decimal TongChatDam { get; set; } = 0;
        public decimal TongTinhBot { get; set; } = 0;
        public decimal TongChatBeo { get; set; } = 0;
        public decimal TongChatXo { get; set; } = 0;

        public bool TaoTuAI { get; set; } = false;
        public bool DaXacNhan { get; set; } = false;
        public bool DaXoa { get; set; } = false; // Soft Delete theo BR-35

        // Navigation
        public virtual NguoiDung NguoiDung { get; set; } = null!;
        public virtual ICollection<ThanhPhanBuaAn> ThanhPhans { get; set; } = new List<ThanhPhanBuaAn>();
        public virtual ICollection<LichSuNhanDienAI> LichSuNhanDiens { get; set; } = new List<LichSuNhanDienAI>();
    }
}
