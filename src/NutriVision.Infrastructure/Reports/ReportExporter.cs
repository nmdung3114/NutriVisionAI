using System.Globalization;
using System.Text;
using NutriVision.Application.Modules.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NutriVision.Infrastructure.Reports;

public class ReportExporter : IReportExporter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly string[] Muc = ["Thap", "DatChuan", "Cao"];
    private static string Label(string v) => v switch { "Thap" => "Thấp", "DatChuan" => "Đạt chuẩn", "Cao" => "Cao", _ => v };
    private static string TrendLabel(string v) => v switch { "Tang" => "Tăng", "Giam" => "Giảm", "OnDinh" => "Ổn định", _ => "Chưa đủ dữ liệu" };

    static ReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.EnableDebugging = false;
    }

    public byte[] ExportCsv(ReportDto r, string hoTen)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Ngày,Calo (kcal),Chất đạm (g),Tinh bột (g),Chất béo (g),Chất xơ (g),Số bữa ăn,Mục tiêu calo (kcal)");
        foreach (var d in r.TheoNgay)
            sb.AppendLine(string.Join(',', d.Ngay.ToString("yyyy-MM-dd"), N(d.Calo), N(d.ChatDam), N(d.TinhBot), N(d.ChatBeo), N(d.ChatXo), d.SoBuaAn, N(d.MucTieuCalo)));
        sb.AppendLine();
        sb.AppendLine($"{Esc("Người dùng")},{Esc(hoTen)}");
        sb.AppendLine($"{Esc("Khoảng thời gian")},{r.From:yyyy-MM-dd} → {r.To:yyyy-MM-dd}");
        sb.AppendLine($"{Esc("Trung bình/ngày (kcal)")},{N(r.TrungBinhNgay.Calo)}");
        sb.AppendLine($"{Esc("Trung bình đạm/tinh bột/béo/xơ (g)")},{N(r.TrungBinhNgay.ChatDam)} / {N(r.TrungBinhNgay.TinhBot)} / {N(r.TrungBinhNgay.ChatBeo)} / {N(r.TrungBinhNgay.ChatXo)}");
        // BOM giúp Excel đọc đúng UTF-8 tiếng Việt.
        return new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string N(decimal v) => v.ToString("0.##", Inv);

    /// <summary>Escape CSV + chống CSV/formula injection (ô bắt đầu bằng = + - @).</summary>
    private static string Esc(string s)
    {
        if (s.Length > 0 && "=+-@\t\r".Contains(s[0])) s = "'" + s;
        return s.Contains(',') || s.Contains('"') || s.Contains('\n') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
    }

    public byte[] ExportPdf(ReportDto r, string hoTen)
    {
        var green = Color.FromHex("#168d47");
        return Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(32);
            page.DefaultTextStyle(t => t.FontSize(10).FontFamily(FontFamilies));

            page.Header().Column(c =>
            {
                c.Item().Text("NutriVision AI – Báo cáo dinh dưỡng").FontSize(18).Bold().FontColor(green);
                c.Item().Text($"Người dùng: {hoTen}  |  Từ {r.From:dd/MM/yyyy} đến {r.To:dd/MM/yyyy}").FontColor(Colors.Grey.Darken1);
                c.Item().PaddingTop(6).LineHorizontal(1).LineColor(green);
            });

            page.Content().PaddingVertical(10).Column(col =>
            {
                col.Spacing(10);
                col.Item().Text("Tổng quan").FontSize(13).Bold();
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); });
                    Head(t, "Chỉ số", "Trung bình/ngày", "Đánh giá");
                    Row(t, "Calo (kcal)", $"{N(r.TrungBinhNgay.Calo)} ({N(r.PhanTramMucTieu)}% mục tiêu {N(r.MucTieuCaloTrungBinh)})", Label(r.DanhGia.Calo));
                    Row(t, "Chất đạm (g)", N(r.TrungBinhNgay.ChatDam), Label(r.DanhGia.ChatDam));
                    Row(t, "Tinh bột (g)", N(r.TrungBinhNgay.TinhBot), Label(r.DanhGia.TinhBot));
                    Row(t, "Chất béo (g)", N(r.TrungBinhNgay.ChatBeo), Label(r.DanhGia.ChatBeo));
                    Row(t, "Chất xơ (g)", N(r.TrungBinhNgay.ChatXo), Label(r.DanhGia.ChatXo));
                });
                col.Item().Text($"Số ngày có dữ liệu: {r.SoNgayCoDuLieu}/{r.SoNgay}  •  Tổng số bữa: {r.TongSoBuaAn}  •  Xu hướng: {TrendLabel(r.XuHuong)}  •  Cơ cấu năng lượng: đạm {N(r.TyLeNangLuong.ChatDam)}% / tinh bột {N(r.TyLeNangLuong.TinhBot)}% / béo {N(r.TyLeNangLuong.ChatBeo)}%");

                col.Item().Text("Chi tiết theo ngày").FontSize(13).Bold();
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(1.3f); for (var i = 0; i < 6; i++) c.RelativeColumn(); });
                    Head(t, "Ngày", "Calo", "Đạm", "Tinh bột", "Béo", "Xơ", "Số bữa");
                    foreach (var d in r.TheoNgay)
                        Row(t, d.Ngay.ToString("dd/MM/yyyy"), N(d.Calo), N(d.ChatDam), N(d.TinhBot), N(d.ChatBeo), N(d.ChatXo), d.SoBuaAn.ToString());
                });

                if (r.KhuyenNghi.Count > 0)
                {
                    col.Item().Text("Khuyến nghị").FontSize(13).Bold();
                    foreach (var k in r.KhuyenNghi) col.Item().Text("• " + k.NoiDung);
                }
            });

            page.Footer().AlignCenter().Text(t =>
            {
                t.Span("Trang ").FontColor(Colors.Grey.Medium);
                t.CurrentPageNumber();
                t.Span(" / ").FontColor(Colors.Grey.Medium);
                t.TotalPages();
            });
        })).GeneratePdf();
    }

    // Font có đủ glyph tiếng Việt; QuestPDF tự fallback nếu font đầu tiên không có.
    private static readonly string[] FontFamilies = ["Arial", "DejaVu Sans", "Liberation Sans", "Noto Sans", "Segoe UI", "Lato"];

    private static void Head(TableDescriptor t, params string[] cells)
    {
        t.Header(h =>
        {
            foreach (var c in cells)
                h.Cell().Background(Color.FromHex("#e1f6df")).Padding(4).Text(c).Bold();
        });
    }

    private static void Row(TableDescriptor t, params string[] cells)
    {
        foreach (var c in cells)
            t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c);
    }
}
