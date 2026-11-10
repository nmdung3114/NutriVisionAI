using System;

namespace NutriVision.Api.Models.Entities
{
    public class TongHopDinhDuongNgay
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid NguoiDungId { get; set; }
        public DateOnly Ngay { get; set; }
        
        public decimal TongCalo { get; set; } = 0;
        public decimal TongChatDam { get; set; } = 0;
        public decimal TongTinhBot { get; set; } = 0;
        public decimal TongChatBeo { get; set; } = 0;
        public decimal TongChatXo { get; set; } = 0;
        
        public int SoBuaAnDaGhi { get; set; } = 0;
        public decimal MucTieuCaloNgay { get; set; } = 2000;
        public DateTimeOffset NgayCapNhat { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public virtual NguoiDung NguoiDung { get; set; } = null!;
    }
}
