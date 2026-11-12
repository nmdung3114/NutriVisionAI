using NutriVision.Domain.Common;
using NutriVision.Domain.Constants;

namespace NutriVision.Domain.Entities;

/// <summary>Tài khoản người dùng (bảng NguoiDung).</summary>
public class NguoiDung : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public bool EmailDaXacThuc { get; private set; }
    public string MatKhauHash { get; private set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string TrangThai { get; private set; } = TrangThaiTaiKhoan.Pending;
    public string VaiTro { get; private set; } = Constants.VaiTro.User;
    public int SoLanDangNhapSai { get; private set; }
    public DateTimeOffset? ThoiGianHetHanKhoa { get; private set; }

    /// <summary>Hash SHA-256 của token xác thực email (không lưu token thô).</summary>
    public string? TokenXacThucEmail { get; private set; }
    public DateTimeOffset? ThoiGianHetHanXacThuc { get; private set; }
    public string? TokenDatLaiMatKhau { get; private set; }
    public DateTimeOffset? ThoiGianHetHanDatLai { get; private set; }

    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }

    public HoSoNguoiDung? HoSo { get; set; }
    public ICollection<TokenLamMoi> TokenLamMois { get; set; } = new List<TokenLamMoi>();

    private NguoiDung() { }

    public static NguoiDung TaoMoi(string email, string hoTen, string matKhauHash, DateTimeOffset now) => new()
    {
        Email = email,
        HoTen = hoTen,
        MatKhauHash = matKhauHash,
        TrangThai = TrangThaiTaiKhoan.Pending,
        VaiTro = Constants.VaiTro.User,
        NgayTao = now,
        NgayCapNhat = now,
    };

    /// <summary>Tạo tài khoản đã kích hoạt sẵn (dữ liệu khởi tạo / quản trị).</summary>
    public static NguoiDung TaoDaKichHoat(string email, string hoTen, string matKhauHash, string vaiTro, DateTimeOffset now)
    {
        var u = TaoMoi(email, hoTen, matKhauHash, now);
        u.VaiTro = vaiTro;
        u.KichHoat(now);
        return u;
    }

    public bool DangBiKhoaTamThoi(DateTimeOffset now) => ThoiGianHetHanKhoa.HasValue && ThoiGianHetHanKhoa.Value > now;

    /// <summary>Ghi nhận đăng nhập sai (BR-07). Trả về true nếu lần này làm tài khoản bị khóa tạm thời.</summary>
    public bool GhiNhanDangNhapSai(DateTimeOffset now, int soLanToiDa, TimeSpan thoiGianKhoa)
    {
        // Hết hạn khóa cũ thì đếm lại từ đầu.
        if (ThoiGianHetHanKhoa.HasValue && ThoiGianHetHanKhoa.Value <= now)
        {
            SoLanDangNhapSai = 0;
            ThoiGianHetHanKhoa = null;
        }

        SoLanDangNhapSai++;
        NgayCapNhat = now;
        if (SoLanDangNhapSai >= soLanToiDa)
        {
            ThoiGianHetHanKhoa = now.Add(thoiGianKhoa);
            SoLanDangNhapSai = 0;
            return true;
        }
        return false;
    }

    public void DangNhapThanhCong(DateTimeOffset now)
    {
        SoLanDangNhapSai = 0;
        ThoiGianHetHanKhoa = null;
        NgayCapNhat = now;
    }

    public void KichHoat(DateTimeOffset now)
    {
        EmailDaXacThuc = true;
        if (TrangThai == TrangThaiTaiKhoan.Pending) TrangThai = TrangThaiTaiKhoan.Active;
        NgayCapNhat = now;
    }

    public void DatTokenXacThucEmail(string tokenHash, DateTimeOffset hetHan, DateTimeOffset now)
    {
        TokenXacThucEmail = tokenHash;
        ThoiGianHetHanXacThuc = hetHan;
        NgayCapNhat = now;
    }

    public void DatTokenDatLaiMatKhau(string tokenHash, DateTimeOffset hetHan, DateTimeOffset now)
    {
        TokenDatLaiMatKhau = tokenHash;
        ThoiGianHetHanDatLai = hetHan;
        NgayCapNhat = now;
    }

    public void DoiMatKhau(string matKhauHash, DateTimeOffset now)
    {
        MatKhauHash = matKhauHash;
        TokenDatLaiMatKhau = null;
        ThoiGianHetHanDatLai = null;
        SoLanDangNhapSai = 0;
        ThoiGianHetHanKhoa = null;
        NgayCapNhat = now;
    }

    public void DoiTrangThai(string trangThai, DateTimeOffset now)
    {
        if (!TrangThaiTaiKhoan.IsValid(trangThai)) throw new ArgumentException("Trạng thái không hợp lệ.", nameof(trangThai));
        TrangThai = trangThai;
        NgayCapNhat = now;
    }

    public void DoiVaiTro(string vaiTro, DateTimeOffset now)
    {
        if (!Constants.VaiTro.IsValid(vaiTro)) throw new ArgumentException("Vai trò không hợp lệ.", nameof(vaiTro));
        VaiTro = vaiTro;
        NgayCapNhat = now;
    }
}
