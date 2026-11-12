using Microsoft.EntityFrameworkCore;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Dashboard;
using NutriVision.Application.Modules.Foods;
using NutriVision.Domain.Services;
using NutriVision.Domain.ValueObjects;

namespace NutriVision.Application.Modules.Reports;

public interface IReportExporter
{
    byte[] ExportCsv(ReportDto report, string hoTen);
    byte[] ExportPdf(ReportDto report, string hoTen);
}

public interface IReportService
{
    Task<ReportDto> GetAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task<ExportedFile> ExportAsync(DateOnly? from, DateOnly? to, ExportFormat format, CancellationToken ct = default);
}

public class ReportService(IApplicationDbContext db, ICurrentUser currentUser, AppClock clock, IReportExporter exporter) : IReportService
{
    private const int MaxDays = 366;

    public async Task<ReportDto> GetAsync(DateOnly? fromIn, DateOnly? toIn, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var to = toIn ?? clock.Today;
        var from = fromIn ?? to.AddDays(-6);
        if (from > to) throw ValidationAppException.Single("from", "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        var soNgay = to.DayNumber - from.DayNumber + 1;
        if (soNgay > MaxDays) throw ValidationAppException.Single("from", "Khoảng thời gian báo cáo tối đa 366 ngày.");

        var profileTarget = await db.HoSoNguoiDungs.AsNoTracking().Where(h => h.NguoiDungId == userId).Select(h => (decimal?)h.MucTieuCalo).FirstOrDefaultAsync(ct);
        var currentTarget = profileTarget is > 0 ? profileTarget.Value : HealthMetricsCalculator.DefaultCalorieTarget;

        // BR-38: chỉ dữ liệu đã xác nhận của chính người dùng trong khoảng chọn (bảng tổng hợp chỉ gồm bữa confirmed).
        var rows = await db.TongHopDinhDuongNgays.AsNoTracking()
            .Where(t => t.NguoiDungId == userId && t.Ngay >= from && t.Ngay <= to).ToListAsync(ct);
        var byDay = rows.ToDictionary(r => r.Ngay);

        var daily = Enumerable.Range(0, soNgay).Select(i =>
        {
            var d = from.AddDays(i);
            return byDay.TryGetValue(d, out var r)
                ? new ReportDayDto(d, r.TongCalo, r.TongChatDam, r.TongTinhBot, r.TongChatBeo, r.TongChatXo, r.SoBuaAnDaGhi, r.MucTieuCaloNgay > 0 ? r.MucTieuCaloNgay : currentTarget)
                : new ReportDayDto(d, 0, 0, 0, 0, 0, 0, currentTarget);
        }).ToList();

        var dataDays = daily.Where(d => d.SoBuaAn > 0).ToList();
        var tong = NutritionCalculator.Tong(dataDays.Select(d => new NutritionValues(d.Calo, d.ChatDam, d.TinhBot, d.ChatBeo, d.ChatXo)));
        var avg = dataDays.Count == 0 ? NutritionValues.Zero : (tong * (1m / dataDays.Count)).Round(1);
        var avgTarget = dataDays.Count == 0 ? currentTarget : Math.Round(dataDays.Average(d => d.MucTieuCalo), 0);
        var pct = avgTarget > 0 ? Math.Round(avg.Calo / avgTarget * 100m, 1) : 0m;
        var goals = MacroTargets.TargetGrams(avgTarget);
        var share = MacroTargets.EnergyPercent(tong);

        var assessment = new ReportAssessmentDto(
            MacroTargets.DanhGia(avg.Calo, avgTarget, 0.9m, 1.1m),
            MacroTargets.DanhGia(avg.ChatDam, goals.ChatDam),
            MacroTargets.DanhGia(avg.TinhBot, goals.TinhBot),
            MacroTargets.DanhGia(avg.ChatBeo, goals.ChatBeo),
            MacroTargets.DanhGia(avg.ChatXo, MacroTargets.FiberTargetGrams));

        var over = dataDays.Count(d => d.Calo > d.MucTieuCalo * 1.1m);
        var under = dataDays.Count(d => d.Calo < d.MucTieuCalo * 0.8m);

        var report = new ReportDto(
            from, to, soNgay, dataDays.Count, dataDays.Sum(d => d.SoBuaAn), avgTarget,
            NutritionDto.From(avg), NutritionDto.From(tong), pct, assessment,
            new MacroGramsDto(share.ChatDam, share.TinhBot, share.ChatBeo),
            Trend(dataDays), over, under, daily, []);

        return report with { KhuyenNghi = BuildRecommendations(report, share, goals) };
    }

    private static string Trend(IReadOnlyList<ReportDayDto> dataDays)
    {
        if (dataDays.Count < 4) return "KhongDuDuLieu";
        var half = dataDays.Count / 2;
        var first = dataDays.Take(half).Average(d => d.Calo);
        var second = dataDays.Skip(dataDays.Count - half).Average(d => d.Calo);
        if (first <= 0) return "KhongDuDuLieu";
        var change = (second - first) / first;
        return change > 0.08m ? "Tang" : change < -0.08m ? "Giam" : "OnDinh";
    }

    private static List<RecommendationDto> BuildRecommendations(ReportDto r, MacroShare share, MacroShare goals)
    {
        var list = new List<RecommendationDto>();
        if (r.SoNgayCoDuLieu == 0)
        {
            list.Add(new("info", "Chưa có dữ liệu bữa ăn đã xác nhận trong khoảng thời gian này. Hãy ghi nhận bữa ăn đầu tiên để nhận phân tích."));
            return list;
        }

        var d = r.SoNgayCoDuLieu;
        if (r.SoNgayVuotMucTieu > 0)
            list.Add(new("warning", $"Có {r.SoNgayVuotMucTieu}/{d} ngày bạn ăn vượt mục tiêu calo (trên 110%). Hãy giảm khẩu phần tinh bột và đồ chiên rán."));
        if (r.SoNgayThieuNangLuong > 0)
            list.Add(new("warning", $"Có {r.SoNgayThieuNangLuong}/{d} ngày nạp dưới 80% mục tiêu calo. Thiếu năng lượng kéo dài có thể gây mệt mỏi, hãy bổ sung bữa phụ lành mạnh."));
        if (r.SoNgayVuotMucTieu == 0 && r.SoNgayThieuNangLuong == 0)
            list.Add(new("success", $"Tuyệt vời! Cả {d} ngày có dữ liệu đều nằm trong ngưỡng mục tiêu calo."));

        if (r.DanhGia.ChatXo == "Thap")
            list.Add(new("warning", $"Chất xơ trung bình chỉ {r.TrungBinhNgay.ChatXo:0.#} g/ngày (khuyến nghị ≥ {MacroTargets.FiberTargetGrams:0} g). Hãy ăn thêm rau xanh, trái cây và ngũ cốc nguyên hạt."));
        if (r.DanhGia.ChatDam == "Thap")
            list.Add(new("info", $"Lượng đạm trung bình {r.TrungBinhNgay.ChatDam:0.#} g/ngày thấp hơn mục tiêu ~{goals.ChatDam:0} g. Bổ sung thịt nạc, cá, trứng hoặc đậu."));
        if (share.ChatBeo > 35m)
            list.Add(new("warning", $"Chất béo chiếm {share.ChatBeo:0.#}% năng lượng (khuyến nghị 20–35%). Hạn chế món chiên rán và nhiều dầu mỡ."));
        if (share.TinhBot > 65m)
            list.Add(new("info", $"Tinh bột chiếm {share.TinhBot:0.#}% năng lượng, khá cao. Hãy cân đối thêm đạm và rau."));

        if (r.XuHuong == "Tang") list.Add(new("info", "Xu hướng calo của bạn đang tăng dần trong kỳ này."));
        else if (r.XuHuong == "Giam") list.Add(new("info", "Xu hướng calo của bạn đang giảm dần trong kỳ này."));

        if (r.SoNgayCoDuLieu * 2 < r.SoNgay)
            list.Add(new("info", $"Bạn mới ghi nhận {r.SoNgayCoDuLieu}/{r.SoNgay} ngày. Ghi nhận đều đặn giúp báo cáo chính xác hơn."));
        return list;
    }

    public async Task<ExportedFile> ExportAsync(DateOnly? from, DateOnly? to, ExportFormat format, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var report = await GetAsync(from, to, ct); // BR-41: chỉ dữ liệu của chính user
        var hoTen = await db.NguoiDungs.AsNoTracking().Where(u => u.Id == userId).Select(u => u.HoTen).FirstAsync(ct);
        var stem = $"nutrivision-bao-cao-{report.From:yyyyMMdd}-{report.To:yyyyMMdd}";
        return format == ExportFormat.Pdf
            ? new ExportedFile(exporter.ExportPdf(report, hoTen), "application/pdf", stem + ".pdf")
            : new ExportedFile(exporter.ExportCsv(report, hoTen), "text/csv; charset=utf-8", stem + ".csv");
    }
}
