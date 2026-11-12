namespace NutriVision.Application.Modules.Profile;

public record UpdateProfileRequest(
    string HoTen, DateOnly NgaySinh, string GioiTinh, decimal ChieuCao, decimal CanNang, string MucDoVanDong, decimal? MucTieuCalo);

public record ProfileDto(
    string HoTen, string Email, string? AnhDaiDienUrl, bool DaDayDu,
    DateOnly? NgaySinh, int? Tuoi, string? GioiTinh, decimal? ChieuCao, decimal? CanNang, string? MucDoVanDong,
    decimal? ChiSoBMI, string? PhanLoaiBMI, decimal? ChiSoBMR, decimal? ChiSoTDEE, decimal MucTieuCalo, DateTimeOffset? NgayCapNhat);

public record AvatarResult(string AnhDaiDienUrl);
