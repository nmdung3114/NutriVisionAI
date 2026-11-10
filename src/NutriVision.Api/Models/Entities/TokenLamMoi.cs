using System;

namespace NutriVision.Api.Models.Entities
{
    public class TokenLamMoi
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid NguoiDungId { get; set; }
        public string MaToken { get; set; } = string.Empty;
        public DateTimeOffset ThoiGianHetHan { get; set; }
        public bool DaSuDung { get; set; } = false;
        public bool DaThuHoi { get; set; } = false;
        public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public virtual NguoiDung NguoiDung { get; set; } = null!;
    }
}
