using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Audit;
using NutriVision.Application.Settings;
using NutriVision.Domain.Constants;
using NutriVision.Domain.Entities;

namespace NutriVision.Application.Modules.Auth;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<VerifyEmailResult> VerifyEmailAsync(string token, CancellationToken ct = default);
    Task ResendVerificationAsync(string email, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string? refreshToken, CancellationToken ct = default);
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<AuthResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default);
    Task<AccountInfo> GetAccountAsync(CancellationToken ct = default);
}

public class AuthService(
    IApplicationDbContext db,
    IPasswordHasher hasher,
    ITokenService tokens,
    IEmailSender email,
    IFileUrlSigner signer,
    ICurrentUser currentUser,
    IAuditService audit,
    AppClock clock,
    IOptions<AuthPolicySettings> policyOptions,
    IOptions<AppSettings> appOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly AuthPolicySettings _policy = policyOptions.Value;
    private readonly AppSettings _app = appOptions.Value;

    private const int RefreshReuseGraceSeconds = 30;

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    // ---------- FR-01 Đăng ký ----------
    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var mail = NormalizeEmail(request.Email);
        if (await db.NguoiDungs.AnyAsync(u => u.Email == mail, ct))
            throw new AppException(ErrorCodes.EmailExists, "Email đã được đăng ký.", 409);

        var now = clock.UtcNow;
        var user = NguoiDung.TaoMoi(mail, request.HoTen.Trim(), hasher.Hash(request.MatKhau), now);
        var raw = tokens.GenerateOpaqueToken();
        user.DatTokenXacThucEmail(tokens.HashToken(raw), now.AddHours(_policy.EmailVerifyHours), now);

        db.NguoiDungs.Add(user);
        audit.Ghi(HanhDong.DangKy, "NguoiDung", user.Id.ToString(), $"Đăng ký tài khoản {mail}", user.Id);
        await db.SaveChangesAsync(ct);

        var sent = await TrySendVerifyEmailAsync(user, raw, ct);
        return new RegisterResult(mail, sent);
    }

    private async Task<bool> TrySendVerifyEmailAsync(NguoiDung user, string rawToken, CancellationToken ct)
    {
        try
        {
            var link = $"{_app.FrontendUrl.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(rawToken)}";
            var (subject, html) = AuthEmailTemplates.Verify(user.HoTen, link, _policy.EmailVerifyHours);
            await email.SendAsync(user.Email, user.HoTen, subject, html, ct);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gửi email xác thực thất bại cho user {UserId}", user.Id);
            return false;
        }
    }

    // ---------- FR-02 Xác thực email ----------
    public async Task<VerifyEmailResult> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        var hash = tokens.HashToken(token.Trim());
        var user = await db.NguoiDungs.FirstOrDefaultAsync(u => u.TokenXacThucEmail == hash, ct)
                   ?? throw new AppException(ErrorCodes.TokenInvalid, "Liên kết xác thực không hợp lệ.", 400);

        // Token được giữ lại sau khi dùng để phân biệt "đã sử dụng" với "không hợp lệ" (BR-04, AC-05).
        if (user.EmailDaXacThuc)
            throw new AppException(ErrorCodes.TokenUsed, "Liên kết đã được sử dụng. Bạn có thể đăng nhập ngay.", 400, new VerifyEmailResult(true));

        if (user.ThoiGianHetHanXacThuc is null || user.ThoiGianHetHanXacThuc < clock.UtcNow)
            throw new AppException(ErrorCodes.TokenExpired, "Liên kết đã hết hạn. Vui lòng yêu cầu gửi lại email xác thực.", 400);

        user.KichHoat(clock.UtcNow);
        audit.Ghi(HanhDong.XacThucEmail, "NguoiDung", user.Id.ToString(), "Xác thực email thành công", user.Id);
        await db.SaveChangesAsync(ct);
        return new VerifyEmailResult(false);
    }

    public async Task ResendVerificationAsync(string emailAddress, CancellationToken ct = default)
    {
        var mail = NormalizeEmail(emailAddress);
        var user = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == mail, ct);
        if (user is null || user.EmailDaXacThuc) return; // không tiết lộ (BR-11)

        var now = clock.UtcNow;
        // Giới hạn gửi lại: token hiện tại tạo chưa quá cooldown thì bỏ qua.
        if (user.ThoiGianHetHanXacThuc is { } exp &&
            exp.AddHours(-_policy.EmailVerifyHours) > now.AddSeconds(-_policy.ResendCooldownSeconds)) return;

        var raw = tokens.GenerateOpaqueToken();
        user.DatTokenXacThucEmail(tokens.HashToken(raw), now.AddHours(_policy.EmailVerifyHours), now);
        await db.SaveChangesAsync(ct);
        await TrySendVerifyEmailAsync(user, raw, ct);
    }

    // ---------- FR-03 Đăng nhập ----------
    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var mail = NormalizeEmail(request.Email);
        var now = clock.UtcNow;
        var user = await db.NguoiDungs.Include(u => u.HoSo).FirstOrDefaultAsync(u => u.Email == mail, ct);

        if (user is null)
        {
            hasher.Hash(request.MatKhau); // cân bằng thời gian phản hồi, hạn chế dò email
            audit.Ghi(HanhDong.DangNhapThatBai, "NguoiDung", null, $"Đăng nhập thất bại: email không tồn tại ({Truncate(mail)})");
            await db.SaveChangesAsync(ct);
            throw InvalidCredentials();
        }

        if (user.DangBiKhoaTamThoi(now))
            throw TempLocked(user.ThoiGianHetHanKhoa!.Value);

        if (!hasher.Verify(request.MatKhau, user.MatKhauHash))
        {
            var lockedNow = user.GhiNhanDangNhapSai(now, _policy.MaxFailedAttempts, TimeSpan.FromMinutes(_policy.LockoutMinutes));
            audit.Ghi(HanhDong.DangNhapThatBai, "NguoiDung", user.Id.ToString(), "Đăng nhập thất bại: sai mật khẩu", user.Id);
            if (lockedNow)
                audit.Ghi(HanhDong.KhoaTaiKhoanTamThoi, "NguoiDung", user.Id.ToString(),
                    $"Khóa {_policy.LockoutMinutes} phút do đăng nhập sai {_policy.MaxFailedAttempts} lần", user.Id);
            await db.SaveChangesAsync(ct);
            throw lockedNow ? TempLocked(user.ThoiGianHetHanKhoa!.Value) : InvalidCredentials();
        }

        if (user.TrangThai == TrangThaiTaiKhoan.Locked)
            throw new AppException(ErrorCodes.AccountDisabled, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.", 403);

        if (!user.EmailDaXacThuc || user.TrangThai == TrangThaiTaiKhoan.Pending)
            throw new AppException(ErrorCodes.EmailNotVerified, "Vui lòng xác thực email trước khi đăng nhập.", 403);

        user.DangNhapThanhCong(now);
        audit.Ghi(HanhDong.DangNhap, "NguoiDung", user.Id.ToString(), "Đăng nhập thành công", user.Id);
        var result = await IssueTokensAsync(user, ct);
        return result;
    }

    private static AppException InvalidCredentials() =>
        new(ErrorCodes.InvalidCredentials, "Email hoặc mật khẩu không chính xác.", 401);

    private AppException TempLocked(DateTimeOffset until) =>
        new(ErrorCodes.AccountTempLocked,
            $"Tài khoản tạm thời bị khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau {Math.Max(1, (int)Math.Ceiling((until - clock.UtcNow).TotalMinutes))} phút.",
            403, new { moKhoaLuc = until });

    private async Task<AuthResult> IssueTokensAsync(NguoiDung user, CancellationToken ct)
    {
        var (access, expires) = tokens.CreateAccessToken(user);
        var rawRefresh = tokens.GenerateOpaqueToken();
        var now = clock.UtcNow;
        db.TokenLamMois.Add(new TokenLamMoi
        {
            NguoiDungId = user.Id,
            MaToken = tokens.HashToken(rawRefresh),
            ThoiGianHetHan = now.AddDays(_policy.RefreshTokenDays),
            NgayTao = now,
        });
        await db.SaveChangesAsync(ct);

        var avatar = user.HoSo?.DuongDanAnhDaiDien ?? (await db.HoSoNguoiDungs.AsNoTracking().Where(h => h.NguoiDungId == user.Id).Select(h => h.DuongDanAnhDaiDien).FirstOrDefaultAsync(ct));
        return new AuthResult(access, rawRefresh, expires, ToUserInfo(user, avatar));
    }

    private UserInfo ToUserInfo(NguoiDung u, string? avatarPath) =>
        new(u.Id, u.Email, u.HoTen, u.VaiTro, u.TrangThai, u.EmailDaXacThuc, signer.Sign(avatarPath));

    // ---------- Refresh token (xoay vòng + phát hiện dùng lại) ----------
    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = tokens.HashToken(refreshToken.Trim());
        var stored = await db.TokenLamMois.Include(t => t.NguoiDung).ThenInclude(u => u!.HoSo)
            .FirstOrDefaultAsync(t => t.MaToken == hash, ct);
        var now = clock.UtcNow;

        if (stored?.NguoiDung is null)
            throw new UnauthorizedAppException("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.", ErrorCodes.TokenInvalid);

        if (stored.DaThuHoi)
            throw new UnauthorizedAppException("Phiên đăng nhập đã hết hiệu lực. Vui lòng đăng nhập lại.", ErrorCodes.TokenInvalid);

        if (stored.DaSuDung)
        {
            // Khoảng ân hạn: trình duyệt có thể gửi lại token vừa xoay vòng khi request trước bị hủy (tải lại trang, nhiều tab).
            // Nếu token kế tiếp mới được cấp trong vòng 30 giây → coi là tranh chấp hợp lệ. Ngoài ra: nghi ngờ bị đánh cắp → thu hồi toàn bộ.
            var graceStart = now.AddSeconds(-RefreshReuseGraceSeconds);
            var recentRotation = await db.TokenLamMois.AnyAsync(t => t.NguoiDungId == stored.NguoiDungId && !t.DaThuHoi
                && t.NgayTao >= graceStart && t.NgayTao >= stored.NgayTao && t.Id != stored.Id, ct);
            if (!recentRotation)
            {
                await RevokeAllAsync(stored.NguoiDungId, ct);
                throw new UnauthorizedAppException("Phiên đăng nhập đã hết hiệu lực. Vui lòng đăng nhập lại.", ErrorCodes.TokenInvalid);
            }
        }

        if (stored.ThoiGianHetHan <= now)
            throw new UnauthorizedAppException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", ErrorCodes.TokenExpired);

        var user = stored.NguoiDung;
        if (user.TrangThai != TrangThaiTaiKhoan.Active)
            throw new UnauthorizedAppException("Tài khoản không còn hoạt động.", ErrorCodes.AccountDisabled);

        stored.DaSuDung = true;
        return await IssueTokensAsync(user, ct);
    }

    // ---------- FR-04 Đăng xuất ----------
    public async Task LogoutAsync(string? refreshToken, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var hash = tokens.HashToken(refreshToken.Trim());
            var stored = await db.TokenLamMois.FirstOrDefaultAsync(t => t.MaToken == hash && t.NguoiDungId == userId, ct);
            if (stored is not null) stored.DaThuHoi = true;
        }
        audit.Ghi(HanhDong.DangXuat, "NguoiDung", userId.ToString(), "Đăng xuất");
        await db.SaveChangesAsync(ct);
    }

    private async Task RevokeAllAsync(Guid userId, CancellationToken ct)
    {
        var active = await db.TokenLamMois.Where(t => t.NguoiDungId == userId && !t.DaThuHoi && !t.DaSuDung).ToListAsync(ct);
        foreach (var t in active) t.DaThuHoi = true;
        await db.SaveChangesAsync(ct);
    }

    // ---------- FR-05 Quên mật khẩu ----------
    public async Task ForgotPasswordAsync(string emailAddress, CancellationToken ct = default)
    {
        var mail = NormalizeEmail(emailAddress);
        var user = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == mail, ct);
        if (user is null || user.TrangThai == TrangThaiTaiKhoan.Locked) return; // luôn trả thông báo chung (BR-11)

        var now = clock.UtcNow;
        if (user.ThoiGianHetHanDatLai is { } exp &&
            exp.AddMinutes(-_policy.PasswordResetMinutes) > now.AddSeconds(-_policy.ResendCooldownSeconds)) return;

        var raw = tokens.GenerateOpaqueToken();
        user.DatTokenDatLaiMatKhau(tokens.HashToken(raw), now.AddMinutes(_policy.PasswordResetMinutes), now);
        audit.Ghi(HanhDong.QuenMatKhau, "NguoiDung", user.Id.ToString(), "Yêu cầu đặt lại mật khẩu", user.Id);
        await db.SaveChangesAsync(ct);

        try
        {
            var link = $"{_app.FrontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(raw)}";
            var (subject, html) = AuthEmailTemplates.Reset(user.HoTen, link, _policy.PasswordResetMinutes);
            await email.SendAsync(user.Email, user.HoTen, subject, html, ct);
        }
        catch (Exception ex)
        {
            // Không làm lộ trạng thái tài khoản cho client (UC-05 5b).
            logger.LogError(ex, "Gửi email đặt lại mật khẩu thất bại cho user {UserId}", user.Id);
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var hash = tokens.HashToken(request.Token.Trim());
        var user = await db.NguoiDungs.FirstOrDefaultAsync(u => u.TokenDatLaiMatKhau == hash, ct)
                   ?? throw new AppException(ErrorCodes.TokenInvalid, "Liên kết không hợp lệ hoặc đã được sử dụng. Vui lòng yêu cầu đặt lại mật khẩu mới.", 400);

        var now = clock.UtcNow;
        if (user.ThoiGianHetHanDatLai is null || user.ThoiGianHetHanDatLai < now)
            throw new AppException(ErrorCodes.TokenExpired, "Liên kết đã hết hạn. Vui lòng yêu cầu đặt lại mật khẩu mới.", 400);

        if (hasher.Verify(request.MatKhauMoi, user.MatKhauHash))
            throw new AppException(ErrorCodes.SamePassword, "Mật khẩu mới không được trùng mật khẩu cũ.", 400);

        user.DoiMatKhau(hasher.Hash(request.MatKhauMoi), now);
        if (!user.EmailDaXacThuc) user.KichHoat(now); // đã chứng minh quyền sở hữu email
        await RevokeAllNoSaveAsync(user.Id, ct);
        audit.Ghi(HanhDong.DatLaiMatKhau, "NguoiDung", user.Id.ToString(), "Đặt lại mật khẩu qua email", user.Id);
        await db.SaveChangesAsync(ct);
    }

    private async Task RevokeAllNoSaveAsync(Guid userId, CancellationToken ct)
    {
        var active = await db.TokenLamMois.Where(t => t.NguoiDungId == userId && !t.DaThuHoi && !t.DaSuDung).ToListAsync(ct);
        foreach (var t in active) t.DaThuHoi = true;
    }

    // ---------- FR-06 Đổi mật khẩu ----------
    public async Task<AuthResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var user = await db.NguoiDungs.Include(u => u.HoSo).FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new UnauthorizedAppException();

        if (!hasher.Verify(request.MatKhauHienTai, user.MatKhauHash))
            throw new AppException(ErrorCodes.WrongPassword, "Mật khẩu hiện tại không chính xác.", 400);
        if (hasher.Verify(request.MatKhauMoi, user.MatKhauHash))
            throw new AppException(ErrorCodes.SamePassword, "Mật khẩu mới không được trùng mật khẩu cũ.", 400);

        user.DoiMatKhau(hasher.Hash(request.MatKhauMoi), clock.UtcNow);
        await RevokeAllNoSaveAsync(user.Id, ct);
        audit.Ghi(HanhDong.DoiMatKhau, "NguoiDung", user.Id.ToString(), "Đổi mật khẩu", user.Id);
        await db.SaveChangesAsync(ct);
        return await IssueTokensAsync(user, ct); // phiên mới cho thiết bị hiện tại; các phiên khác đã bị hủy (BR-08)
    }

    // ---------- FR-07 ----------
    public async Task<AccountInfo> GetAccountAsync(CancellationToken ct = default)
    {
        var userId = currentUser.RequireUserId();
        var u = await db.NguoiDungs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, ct) ?? throw new UnauthorizedAppException();
        var avatar = await db.HoSoNguoiDungs.AsNoTracking().Where(h => h.NguoiDungId == userId).Select(h => h.DuongDanAnhDaiDien).FirstOrDefaultAsync(ct);
        return new AccountInfo(u.Id, u.Email, u.HoTen, u.VaiTro, u.TrangThai, u.EmailDaXacThuc, signer.Sign(avatar), u.NgayTao);
    }

    private static string Truncate(string s) => s.Length <= 100 ? s : s[..100];
}
