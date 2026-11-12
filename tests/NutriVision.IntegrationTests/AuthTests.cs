using Microsoft.Extensions.DependencyInjection;
using NutriVision.Application.Common;
using Xunit;

namespace NutriVision.IntegrationTests;

public class AuthTests(ApiFactory f) : IntegrationTestBase(f)
{
    private static object Reg(string email, string pw = "Passw0rd1", bool terms = true) =>
        new { hoTen = "Nguyễn A", email, matKhau = pw, xacNhanMatKhau = pw, dongYDieuKhoan = terms };

    [Fact] // AC-01, AC-04, AC-05, AC-08
    public async Task Register_Verify_Login_Flow()
    {
        var c = NewClient();
        var email = $"a{Guid.NewGuid():N}@test.vn";
        var reg = await c.Post("/api/auth/register", Reg(email));
        Assert.Equal(200, reg.Status);
        Assert.True(reg.Data.GetProperty("emailDaGui").GetBoolean());

        var blocked = await c.Login(email, "Passw0rd1"); // BR-03
        Assert.Equal(403, blocked.Status);
        Assert.Equal(ErrorCodes.EmailNotVerified, blocked.Code);

        var token = Factory.Email.TokenFor(email);
        Assert.True((await c.Post("/api/auth/verify-email", new { token })).Success);

        var again = await c.Post("/api/auth/verify-email", new { token }); // BR-04
        Assert.Equal(ErrorCodes.TokenUsed, again.Code);

        var bad = await c.Post("/api/auth/verify-email", new { token = "khong-hop-le" });
        Assert.Equal(ErrorCodes.TokenInvalid, bad.Code);

        var login = await c.Login(email, "Passw0rd1");
        Assert.True(login.Success);
        Assert.Equal("ACTIVE", login.Data.GetProperty("user").GetProperty("trangThai").GetString());
        Assert.Equal("USER", login.Data.GetProperty("user").GetProperty("vaiTro").GetString());
    }

    [Fact] // AC-02, AC-03, BR-02, BR-10
    public async Task Register_Rejects_Duplicate_And_Weak_Password()
    {
        var c = NewClient();
        var dup = await c.Post("/api/auth/register", Reg("user@nutrivision.ai"));
        Assert.Equal(409, dup.Status);
        Assert.Equal(ErrorCodes.EmailExists, dup.Code);

        var weak = await c.Post("/api/auth/register", Reg($"w{Guid.NewGuid():N}@test.vn", "abc"));
        Assert.Equal(400, weak.Status);
        Assert.Equal(ErrorCodes.Validation, weak.Code);
        Assert.Contains("8 ký tự", weak.Json.GetProperty("data").GetProperty("fieldErrors").GetProperty("matKhau")[0].GetString());

        var noTerms = await c.Post("/api/auth/register", Reg($"w{Guid.NewGuid():N}@test.vn", "Passw0rd1", false));
        Assert.Equal(400, noTerms.Status);
    }

    [Fact] // AC-06: token hết hạn 24h
    public async Task Expired_Verification_Token_Is_Rejected()
    {
        var c = NewClient();
        var email = $"e{Guid.NewGuid():N}@test.vn";
        await c.Post("/api/auth/register", Reg(email));
        var token = Factory.Email.TokenFor(email);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NutriVision.Infrastructure.Persistence.ApplicationDbContext>();
        var user = db.NguoiDungs.First(u => u.Email == email);
        user.DatTokenXacThucEmail(user.TokenXacThucEmail!, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        var r = await c.Post("/api/auth/verify-email", new { token });
        Assert.Equal(ErrorCodes.TokenExpired, r.Code);
    }

    [Fact] // AC-09, BR-07
    public async Task Five_Wrong_Passwords_Lock_Account_Even_For_Correct_Password()
    {
        var (_, email) = await NewVerifiedUser(withProfile: false);
        var c = NewClient();
        for (var i = 0; i < 4; i++)
            Assert.Equal(ErrorCodes.InvalidCredentials, (await c.Login(email, "Sai-mat-khau1")).Code);
        var fifth = await c.Login(email, "Sai-mat-khau1");
        Assert.Equal(ErrorCodes.AccountTempLocked, fifth.Code);
        var correct = await c.Login(email, "Passw0rd1");
        Assert.Equal(403, correct.Status);
        Assert.Equal(ErrorCodes.AccountTempLocked, correct.Code);
    }

    [Fact]
    public async Task Login_Error_Does_Not_Reveal_Whether_Email_Exists()
    {
        var c = NewClient();
        var a = await c.Login("khong-ton-tai@test.vn", "Passw0rd1");
        var b = await c.Login("user@nutrivision.ai", "Sai-mat-khau1");
        Assert.Equal(a.Json.GetProperty("message").GetString(), b.Json.GetProperty("message").GetString());
        Assert.Equal(401, a.Status);
    }

    [Fact] // AC-10, AC-07
    public async Task Refresh_Rotates_And_Logout_Revokes()
    {
        var (c, _) = await NewVerifiedUser(withProfile: false);
        var oldRefresh = c.RefreshToken!;
        var r = await c.Post("/api/auth/refresh", new { refreshToken = oldRefresh });
        Assert.True(r.Success);
        var newRefresh = r.Data.GetProperty("refreshToken").GetString()!;
        Assert.NotEqual(oldRefresh, newRefresh);

        // Trong khoảng ân hạn 30s (tải lại trang giữa chừng) token cũ vẫn được chấp nhận một lần
        var grace = await c.Post("/api/auth/refresh", new { refreshToken = oldRefresh });
        Assert.True(grace.Success);
        newRefresh = grace.Data.GetProperty("refreshToken").GetString()!;

        // Quá khoảng ân hạn: dùng lại token cũ → bị từ chối và thu hồi toàn bộ phiên (phát hiện đánh cắp)
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NutriVision.Infrastructure.Persistence.ApplicationDbContext>();
            foreach (var t in db.TokenLamMois.ToList()) t.NgayTao = t.NgayTao.AddMinutes(-5);
            await db.SaveChangesAsync();
        }
        var reuse = await c.Post("/api/auth/refresh", new { refreshToken = oldRefresh });
        Assert.Equal(401, reuse.Status);
        var afterReuse = await c.Post("/api/auth/refresh", new { refreshToken = newRefresh });
        Assert.Equal(401, afterReuse.Status);

        var (c2, _) = await NewVerifiedUser(withProfile: false);
        Assert.True((await c2.Post("/api/auth/logout", new { refreshToken = c2.RefreshToken })).Success);
        Assert.Equal(401, (await c2.Post("/api/auth/refresh", new { refreshToken = c2.RefreshToken })).Status);
    }

    [Fact] // AC-11, AC-12, BR-04, BR-08, BR-11
    public async Task Forgot_And_Reset_Password()
    {
        var (c, email) = await NewVerifiedUser(withProfile: false);
        var oldRefresh = c.RefreshToken!;

        var unknown = await c.Post("/api/auth/forgot-password", new { email = "khong-co@test.vn" });
        var known = await c.Post("/api/auth/forgot-password", new { email });
        Assert.Equal(200, unknown.Status);
        Assert.Equal(unknown.Json.GetProperty("message").GetString(), known.Json.GetProperty("message").GetString());

        var token = Factory.Email.TokenFor(email);
        var weak = await c.Post("/api/auth/reset-password", new { token, matKhauMoi = "abc", xacNhanMatKhau = "abc" });
        Assert.Equal(400, weak.Status);

        var ok = await c.Post("/api/auth/reset-password", new { token, matKhauMoi = "NewPassw0rd", xacNhanMatKhau = "NewPassw0rd" });
        Assert.True(ok.Success);
        Assert.Equal(ErrorCodes.TokenInvalid, (await c.Post("/api/auth/reset-password", new { token, matKhauMoi = "Another1234", xacNhanMatKhau = "Another1234" })).Code);

        Assert.Equal(401, (await c.Post("/api/auth/refresh", new { refreshToken = oldRefresh })).Status); // BR-08
        Assert.False((await NewClient().Login(email, "Passw0rd1")).Success);
        Assert.True((await NewClient().Login(email, "NewPassw0rd")).Success);
    }

    [Fact] // AC-13, AC-14, BR-08, BR-09
    public async Task Change_Password()
    {
        var (c, email) = await NewVerifiedUser(withProfile: false);
        var other = NewClient();
        await other.Login(email, "Passw0rd1");

        Assert.Equal(ErrorCodes.WrongPassword, (await c.Post("/api/auth/change-password", new { matKhauHienTai = "Saimatkhau1", matKhauMoi = "NewPassw0rd", xacNhanMatKhau = "NewPassw0rd" })).Code);
        Assert.Equal(ErrorCodes.SamePassword, (await c.Post("/api/auth/change-password", new { matKhauHienTai = "Passw0rd1", matKhauMoi = "Passw0rd1", xacNhanMatKhau = "Passw0rd1" })).Code);

        var ok = await c.Post("/api/auth/change-password", new { matKhauHienTai = "Passw0rd1", matKhauMoi = "NewPassw0rd", xacNhanMatKhau = "NewPassw0rd" });
        Assert.True(ok.Success);
        Assert.Equal(401, (await other.Post("/api/auth/refresh", new { refreshToken = other.RefreshToken })).Status); // thiết bị khác bị đăng xuất
        c.AccessToken = ok.Data.GetProperty("accessToken").GetString();
        Assert.True((await c.Get("/api/auth/me")).Success);
    }

    [Fact] // AC-15, AC-16, BR-12, BR-13
    public async Task Authorization_Rules()
    {
        Assert.Equal(401, (await NewClient().Get("/api/profile")).Status);

        var user = await LoginUser();
        var forbidden = await user.Get("/api/admin/users");
        Assert.Equal(403, forbidden.Status);
        Assert.Equal(ErrorCodes.Forbidden, forbidden.Code);

        var admin = await LoginAdmin();
        Assert.True((await admin.Get("/api/admin/users")).Success);

        var me = await admin.Get("/api/auth/me");
        var selfLock = await admin.Put($"/api/admin/users/{me.Data.GetProperty("id").GetString()}/status", new { trangThai = "LOCKED" });
        Assert.Equal(400, selfLock.Status);
        Assert.Equal(ErrorCodes.SelfActionForbidden, selfLock.Code);
        var selfRole = await admin.Put($"/api/admin/users/{me.Data.GetProperty("id").GetString()}/role", new { vaiTro = "USER" });
        Assert.Equal(ErrorCodes.SelfActionForbidden, selfRole.Code);
    }

    [Fact] // AC-56, AC-57, BR-44
    public async Task Admin_Lock_Role_Logs_And_Immediate_Effect()
    {
        var (victim, email) = await NewVerifiedUser(withProfile: false);
        var admin = await LoginAdmin();
        var list = await admin.Get($"/api/admin/users?q={email}");
        var item = list.Data.GetProperty("items")[0];
        var id = item.GetProperty("id").GetString();
        Assert.False(item.TryGetProperty("matKhauHash", out _));

        Assert.True((await admin.Put($"/api/admin/users/{id}/status", new { trangThai = "LOCKED", lyDo = "vi phạm" })).Success);
        Assert.Equal(401, (await victim.Get("/api/profile")).Status); // access token cũ bị từ chối ngay
        Assert.Equal(ErrorCodes.AccountDisabled, (await NewClient().Login(email, "Passw0rd1")).Code);

        Assert.True((await admin.Put($"/api/admin/users/{id}/status", new { trangThai = "ACTIVE" })).Success);
        Assert.True((await NewClient().Login(email, "Passw0rd1")).Success);

        Assert.True((await admin.Put($"/api/admin/users/{id}/role", new { vaiTro = "ADMIN" })).Success);
        Assert.True((await admin.Put($"/api/admin/users/{id}/role", new { vaiTro = "USER" })).Success);

        var logs = await admin.Get("/api/admin/logs?pageSize=100");
        var types = logs.Data.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("loaiHanhDong").GetString()).ToHashSet();
        Assert.Contains("ADMIN_KHOA_TAI_KHOAN", types);
        Assert.Contains("ADMIN_MO_KHOA_TAI_KHOAN", types);
        Assert.Contains("ADMIN_DOI_VAI_TRO", types);
        Assert.Contains("DANG_NHAP", types);
        var filtered = await admin.Get("/api/admin/logs?loai=ADMIN_DOI_VAI_TRO");
        Assert.All(filtered.Data.GetProperty("items").EnumerateArray(), x => Assert.Equal("ADMIN_DOI_VAI_TRO", x.GetProperty("loaiHanhDong").GetString()));
        Assert.Equal(403, (await victim.Get("/api/admin/logs")).Status);
    }
}
