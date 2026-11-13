namespace NutriVision.Domain.Common;

/// <summary>Thực thể cơ sở: khóa chính kiểu Guid.</summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
