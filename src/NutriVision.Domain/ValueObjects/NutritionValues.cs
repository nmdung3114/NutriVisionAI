namespace NutriVision.Domain.ValueObjects;

/// <summary>Bộ giá trị dinh dưỡng: kcal và gram các chất (đạm, tinh bột, béo, xơ).</summary>
public readonly record struct NutritionValues(decimal Calo, decimal ChatDam, decimal TinhBot, decimal ChatBeo, decimal ChatXo)
{
    public static readonly NutritionValues Zero = new(0, 0, 0, 0, 0);

    public static NutritionValues operator +(NutritionValues a, NutritionValues b) =>
        new(a.Calo + b.Calo, a.ChatDam + b.ChatDam, a.TinhBot + b.TinhBot, a.ChatBeo + b.ChatBeo, a.ChatXo + b.ChatXo);

    public static NutritionValues operator *(NutritionValues a, decimal factor) =>
        new(a.Calo * factor, a.ChatDam * factor, a.TinhBot * factor, a.ChatBeo * factor, a.ChatXo * factor);

    public NutritionValues Round(int decimals = 2) => new(
        Math.Round(Calo, decimals, MidpointRounding.AwayFromZero),
        Math.Round(ChatDam, decimals, MidpointRounding.AwayFromZero),
        Math.Round(TinhBot, decimals, MidpointRounding.AwayFromZero),
        Math.Round(ChatBeo, decimals, MidpointRounding.AwayFromZero),
        Math.Round(ChatXo, decimals, MidpointRounding.AwayFromZero));

    public bool IsEmpty => Calo == 0 && ChatDam == 0 && TinhBot == 0 && ChatBeo == 0 && ChatXo == 0;
}
