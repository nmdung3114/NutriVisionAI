using System;

namespace NutriVision.Api.Models.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset NgayCapNhat { get; set; } = DateTimeOffset.UtcNow;
    }
}
