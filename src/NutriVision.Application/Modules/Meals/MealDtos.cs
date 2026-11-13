using NutriVision.Application.Modules.Foods;

namespace NutriVision.Application.Modules.Meals;

public record NutritionPer100gInput(decimal CaloTren100g, decimal ChatDamTren100g, decimal TinhBotTren100g, decimal ChatBeoTren100g, decimal ChatXoTren100g);

public record ComponentInput(
    string TenThanhPhan, Guid? ThucPhamThamChieuId, decimal KhauPhan, string DonViTinh, NutritionPer100gInput? NhapTay);

public record MealUpsertRequest(string LoaiBuaAn, DateOnly? NgayAn, string TenMonAn, List<ComponentInput> ThanhPhan);

/// <summary>Ăn lại một bữa đã ghi: sao chép thành phần sang ngày/loại bữa mới (mặc định hôm nay, giữ loại bữa cũ).</summary>
public record MealRepeatRequest(string? LoaiBuaAn, DateOnly? NgayAn);

/// <summary>Món hay ăn trong 90 ngày, gộp theo tên; MauBuaAnId là lần ăn gần nhất để "ăn lại".</summary>
public record FrequentMealDto(Guid MauBuaAnId, string TenMonAn, int SoLan, decimal Calo, string LoaiBuaAn, DateOnly LanCuoi, string? HinhAnhUrl);

public record MealComponentDto(
    Guid? Id, Guid? ThucPhamThamChieuId, string TenThanhPhan, decimal KhauPhan, string DonViTinh, decimal KhoiLuongQuyDoiGram,
    decimal LuongCalo, decimal LuongChatDam, decimal LuongTinhBot, decimal LuongChatBeo, decimal LuongChatXo,
    FoodDto? ThamChieu, NutritionDto? DinhDuongTren100g, bool ThieuDuLieuDinhDuong);

public record AiOriginalComponent(string TenThanhPhan, decimal KhauPhan, string DonViTinh, decimal KhoiLuongGram, Guid? ThucPhamThamChieuId);

public record AiOriginal(string TenMonAn, decimal? DoTinCay, List<AiOriginalComponent> ThanhPhan);

public record MealDto(
    Guid Id, string LoaiBuaAn, DateOnly NgayAn, DateTimeOffset ThoiGianGhiNhan, string TenMonAn, string? HinhAnhUrl,
    bool TaoTuAI, bool DaXacNhan, IReadOnlyList<MealComponentDto> ThanhPhan, NutritionDto TongDinhDuong,
    decimal MucTieuCalo, decimal PhanTramNhuCauNgay, decimal? DoTinCay, AiOriginal? AiGoc, bool? NguoiDungCoSua,
    bool AiThatBai, string? ThongBaoAI);

public record MealSummaryDto(
    Guid Id, string LoaiBuaAn, DateOnly NgayAn, DateTimeOffset ThoiGianGhiNhan, string TenMonAn, string? HinhAnhUrl,
    bool TaoTuAI, NutritionDto TongDinhDuong, int SoThanhPhan);

public record DaySummaryDto(DateOnly Ngay, decimal TongCalo, decimal MucTieuCalo, decimal PhanTram, int SoBuaAn);

public class MealHistoryResult
{
    public IReadOnlyList<MealSummaryDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public IReadOnlyList<DaySummaryDto> TongHopNgay { get; init; } = [];
}

public record MealHistoryQuery(DateOnly? From, DateOnly? To, string? LoaiBuaAn, int Page = 1, int PageSize = 20);
