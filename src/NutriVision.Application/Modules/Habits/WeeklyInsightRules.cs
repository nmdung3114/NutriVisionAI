using System.Globalization;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Services;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Application.Modules.Habits;

/// <summary>Một ngày có dữ liệu (đã xác nhận) trong cửa sổ phân tích.</summary>
public record InsightDay(DateOnly Ngay, NutritionValues Tong, decimal MucTieuCalo, IReadOnlyDictionary<string, decimal> CaloTheoBua);

public record InsightDto(string Ma, string Loai, string TieuDe, string NoiDung, string BieuTuong);

/// <summary>
/// Bộ quy tắc nhận xét tuần (rule-based, giải thích được): so sánh 7 ngày gần nhất với mục tiêu cá nhân
/// và với 7 ngày trước đó. Không dùng AI → kết quả ổn định, kiểm thử được.
/// </summary>
public static class WeeklyInsightRules
{
    public const string CanhBao = "CanhBao", ThongTin = "ThongTin", TichCuc = "TichCuc";
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
    private static string K(decimal v) => v.ToString("N0", Vi);
    private static string P(decimal v) => Math.Round(v, 0, MidpointRounding.AwayFromZero).ToString("0", Vi);

    public static IReadOnlyList<InsightDto> Build(IReadOnlyList<InsightDay> week, IReadOnlyList<InsightDay> previousWeek, int windowDays = 7)
    {
        var list = new List<InsightDto>();
        var n = week.Count;
        if (n == 0)
            return [new("NO_DATA", ThongTin, "Chưa có dữ liệu tuần này", "Ghi nhận bữa ăn trong vài ngày để nhận nhận xét về thói quen dinh dưỡng của bạn.", "📝")];
        if (n < 3)
            list.Add(new("FEW_DAYS", ThongTin, $"Mới có {n}/{windowDays} ngày được ghi nhận", "Ghi đủ các ngày trong tuần để nhận xét chính xác hơn.", "📝"));

        // 1. Calo trung bình so với mục tiêu
        var avgCalo = week.Average(d => d.Tong.Calo);
        var avgTarget = week.Average(d => d.MucTieuCalo);
        var pct = avgTarget > 0 ? avgCalo / avgTarget * 100m : 0m;
        if (pct < 85m)
            list.Add(new("CALO_LOW", CanhBao, $"Calo trung bình chỉ đạt {P(pct)}% mục tiêu",
                $"Trung bình {K(avgCalo)} kcal/ngày so với mục tiêu {K(avgTarget)} kcal. Ăn quá ít kéo dài có thể gây mệt mỏi, mất cơ.", "⚠️"));
        else if (pct > 115m)
            list.Add(new("CALO_HIGH", CanhBao, $"Calo trung bình vượt {P(pct - 100m)}% mục tiêu",
                $"Trung bình {K(avgCalo)} kcal/ngày so với mục tiêu {K(avgTarget)} kcal. Thử giảm đồ chiên, đồ ngọt và nước có đường.", "⚠️"));
        else
            list.Add(new("CALO_OK", TichCuc, "Năng lượng nạp vào rất cân đối",
                $"Trung bình {K(avgCalo)} kcal/ngày, đạt {P(pct)}% mục tiêu. Tiếp tục duy trì nhé!", "🎯"));

        // 2. Chất xơ
        var lowFiber = week.Count(d => d.Tong.ChatXo < MacroTargets.FiberTargetGrams);
        if (lowFiber * 2 >= n && n >= 2)
            list.Add(new("FIBER_LOW", CanhBao, $"Thiếu chất xơ {lowFiber}/{n} ngày",
                "Bổ sung rau xanh, trái cây, ngũ cốc nguyên hạt để đạt ≥ 25 g chất xơ mỗi ngày.", "🥦"));
        else if (lowFiber == 0)
            list.Add(new("FIBER_OK", TichCuc, "Đủ chất xơ mỗi ngày", "Tuyệt vời! Bạn đạt ≥ 25 g chất xơ ở tất cả các ngày đã ghi.", "🥦"));

        // 3. Chất đạm (dưới 80% mục tiêu)
        var lowProtein = week.Count(d => d.Tong.ChatDam < MacroTargets.TargetGrams(d.MucTieuCalo).ChatDam * 0.8m);
        if (lowProtein * 2 >= n && n >= 2)
            list.Add(new("PROTEIN_LOW", CanhBao, $"Thiếu đạm {lowProtein}/{n} ngày",
                "Thêm trứng, cá, thịt nạc, đậu phụ hoặc sữa chua vào các bữa chính.", "💪"));

        // 4. Tỷ lệ năng lượng từ chất béo / tinh bột
        var share = MacroTargets.EnergyPercent(NutritionCalculator.Tong(week.Select(d => d.Tong)));
        if (share.ChatBeo > 35m)
            list.Add(new("FAT_HIGH", CanhBao, $"Chất béo chiếm {P(share.ChatBeo)}% năng lượng",
                "Khuyến nghị khoảng 30%. Ưu tiên luộc, hấp thay vì chiên rán.", "🍳"));
        else if (share.TinhBot > 60m)
            list.Add(new("CARB_HIGH", ThongTin, $"Tinh bột chiếm {P(share.TinhBot)}% năng lượng",
                "Khuyến nghị khoảng 50%. Có thể giảm bớt cơm, bún, bánh mì và tăng rau, đạm.", "🍚"));

        // 5. Phân bổ bữa: bữa tối quá nặng, bỏ bữa sáng
        var totalCalo = week.Sum(d => d.Tong.Calo);
        var dinner = week.Sum(d => d.CaloTheoBua.GetValueOrDefault(LoaiBuaAn.Toi));
        if (totalCalo > 0 && dinner / totalCalo > 0.4m)
            list.Add(new("DINNER_HEAVY", CanhBao, $"Bữa tối chiếm {P(dinner / totalCalo * 100m)}% calo",
                "Ăn tối quá nhiều dễ gây tích mỡ và khó ngủ. Thử chuyển bớt sang bữa sáng, trưa.", "🌙"));
        var skippedBreakfast = week.Count(d => d.CaloTheoBua.GetValueOrDefault(LoaiBuaAn.Sang) <= 0);
        if (skippedBreakfast >= 3)
            list.Add(new("NO_BREAKFAST", ThongTin, $"Bỏ bữa sáng {skippedBreakfast}/{n} ngày",
                "Bữa sáng giúp ổn định đường huyết và hạn chế ăn bù vào buổi tối.", "🌅"));

        // 6. Xu hướng so với tuần trước
        if (n >= 3 && previousWeek.Count >= 3)
        {
            var prevAvg = previousWeek.Average(d => d.Tong.Calo);
            if (prevAvg > 0)
            {
                var change = (avgCalo - prevAvg) / prevAvg * 100m;
                if (Math.Abs(change) >= 10m)
                    list.Add(new("TREND", ThongTin, $"Calo {(change > 0 ? "tăng" : "giảm")} {P(Math.Abs(change))}% so với tuần trước",
                        $"Tuần trước trung bình {K(prevAvg)} kcal/ngày, tuần này {K(avgCalo)} kcal/ngày.", change > 0 ? "📈" : "📉"));
            }
        }

        var order = new Dictionary<string, int> { [CanhBao] = 0, [ThongTin] = 1, [TichCuc] = 2 };
        return list.OrderBy(i => order[i.Loai]).ToList();
    }
}
