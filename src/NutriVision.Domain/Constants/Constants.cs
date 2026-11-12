namespace NutriVision.Domain.Constants;

/// <summary>Vai trò người dùng (BR-12, BR-43).</summary>
public static class VaiTro
{
    public const string User = "USER";
    public const string Admin = "ADMIN";
    public static readonly string[] All = [User, Admin];
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}

/// <summary>Trạng thái tài khoản. PENDING = chưa xác thực email (BR-03); LOCKED = bị admin khóa.</summary>
public static class TrangThaiTaiKhoan
{
    public const string Pending = "PENDING";
    public const string Active = "ACTIVE";
    public const string Locked = "LOCKED";
    public static readonly string[] All = [Pending, Active, Locked];
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}

/// <summary>Loại bữa ăn (FR-13).</summary>
public static class LoaiBuaAn
{
    public const string Sang = "Sang";
    public const string Trua = "Trua";
    public const string Toi = "Toi";
    public const string Phu = "Phu";
    public static readonly string[] All = [Sang, Trua, Toi, Phu];
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}

public static class GioiTinh
{
    public const string Nam = "Nam";
    public const string Nu = "Nu";
    public static readonly string[] All = [Nam, Nu];
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}

/// <summary>Mức độ vận động và hệ số nhân vào BMR để ra TDEE.</summary>
public static class MucDoVanDong
{
    public const string ItVanDong = "ItVanDong";
    public const string VanDongNhe = "VanDongNhe";
    public const string VuaPhai = "VuaPhai";
    public const string NangDong = "NangDong";
    public const string RatNangDong = "RatNangDong";

    public static readonly IReadOnlyDictionary<string, decimal> HeSo = new Dictionary<string, decimal>
    {
        [ItVanDong] = 1.2m,
        [VanDongNhe] = 1.375m,
        [VuaPhai] = 1.55m,
        [NangDong] = 1.725m,
        [RatNangDong] = 1.9m,
    };

    public static bool IsValid(string? value) => value is not null && HeSo.ContainsKey(value);
}

/// <summary>Các loại hành động ghi vào NhatKyHeThong (BR-44).</summary>
public static class HanhDong
{
    public const string DangKy = "DANG_KY";
    public const string XacThucEmail = "XAC_THUC_EMAIL";
    public const string DangNhap = "DANG_NHAP";
    public const string DangNhapThatBai = "DANG_NHAP_THAT_BAI";
    public const string DangXuat = "DANG_XUAT";
    public const string KhoaTaiKhoanTamThoi = "KHOA_TAM_THOI";
    public const string QuenMatKhau = "QUEN_MAT_KHAU";
    public const string DatLaiMatKhau = "DAT_LAI_MAT_KHAU";
    public const string DoiMatKhau = "DOI_MAT_KHAU";
    public const string CapNhatHoSo = "CAP_NHAT_HO_SO";
    public const string GhiCanNang = "GHI_CAN_NANG";
    public const string XacNhanBuaAn = "XAC_NHAN_BUA_AN";
    public const string SuaBuaAn = "SUA_BUA_AN";
    public const string XoaBuaAn = "XOA_BUA_AN";
    public const string NhanDienAI = "NHAN_DIEN_AI";
    public const string AdminKhoaTaiKhoan = "ADMIN_KHOA_TAI_KHOAN";
    public const string AdminMoKhoaTaiKhoan = "ADMIN_MO_KHOA_TAI_KHOAN";
    public const string AdminDoiVaiTro = "ADMIN_DOI_VAI_TRO";
}
