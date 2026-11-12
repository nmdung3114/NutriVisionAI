using System.Globalization;
using System.Text;

namespace NutriVision.Domain.Services;

/// <summary>Chuẩn hóa chuỗi tiếng Việt (bỏ dấu, thường hóa) để so khớp thực phẩm.</summary>
public static class TextNormalizer
{
    public static string BoDau(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var formD = input.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var c in formD)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(c switch { 'đ' => 'd', 'Đ' => 'D', _ => c });
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Bỏ dấu, thường, chỉ giữ chữ-số và khoảng trắng đơn.</summary>
    public static string Chuan(string? input)
    {
        var s = BoDau(input).ToLowerInvariant();
        var sb = new StringBuilder(s.Length);
        var lastSpace = true;
        foreach (var c in s)
        {
            if (char.IsLetterOrDigit(c)) { sb.Append(c); lastSpace = false; }
            else if (!lastSpace) { sb.Append(' '); lastSpace = true; }
        }
        return sb.ToString().Trim();
    }
}
