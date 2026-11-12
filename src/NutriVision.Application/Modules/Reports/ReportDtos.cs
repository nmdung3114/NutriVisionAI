using NutriVision.Application.Modules.Dashboard;
using NutriVision.Application.Modules.Foods;

namespace NutriVision.Application.Modules.Reports;

public record ReportAssessmentDto(string Calo, string ChatDam, string TinhBot, string ChatBeo, string ChatXo);
public record ReportDayDto(DateOnly Ngay, decimal Calo, decimal ChatDam, decimal TinhBot, decimal ChatBeo, decimal ChatXo, int SoBuaAn, decimal MucTieuCalo);
public record RecommendationDto(string Muc, string NoiDung);

public record ReportDto(
    DateOnly From, DateOnly To, int SoNgay, int SoNgayCoDuLieu, int TongSoBuaAn, decimal MucTieuCaloTrungBinh,
    NutritionDto TrungBinhNgay, NutritionDto Tong, decimal PhanTramMucTieu, ReportAssessmentDto DanhGia,
    MacroGramsDto TyLeNangLuong, string XuHuong, int SoNgayVuotMucTieu, int SoNgayThieuNangLuong,
    IReadOnlyList<ReportDayDto> TheoNgay, IReadOnlyList<RecommendationDto> KhuyenNghi);

public enum ExportFormat { Csv, Pdf }

public sealed record ExportedFile(byte[] Content, string ContentType, string FileName);
