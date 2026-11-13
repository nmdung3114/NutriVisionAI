namespace NutriVision.Application.Modules.Admin;

public record AdminUserDto(Guid Id, string Email, string HoTen, string VaiTro, string TrangThai, bool EmailDaXacThuc, DateTimeOffset NgayTao);
public record ChangeStatusRequest(string TrangThai, string? LyDo);
public record ChangeRoleRequest(string VaiTro);

public record AdminLogDto(
    Guid Id, Guid? NguoiDungThucHienId, string? EmailNguoiThucHien, string LoaiHanhDong, string? BangTacDong,
    string? IdDoiTuong, string? ChiTiet, string? DiaChiIP, DateTimeOffset NgayTao);

public record DayCountDto(DateOnly Ngay, int SoLuong);

public record AdminStatisticsDto(
    int TongNguoiDung, int NguoiDungHoatDong, int NguoiDungBiKhoa, int TongBuaAn, int SoAnhAIXuLy, int SoLanAIThatBai,
    decimal? TyLeAIChinhXacPhanTram, int? ThoiGianXuLyAITrungBinhMs,
    IReadOnlyList<DayCountDto> NguoiDungMoiTheoNgay, IReadOnlyList<DayCountDto> BuaAnTheoNgay, IReadOnlyList<DayCountDto> AiTheoNgay);
