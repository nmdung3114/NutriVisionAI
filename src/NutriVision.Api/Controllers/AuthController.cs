using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NutriVision.Application.Modules.Auth;

namespace NutriVision.Api.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService auth) : ApiControllerBase
{
    [HttpPost("register"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var r = await auth.RegisterAsync(request, ct);
        return Success(r, r.EmailDaGui
            ? "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản."
            : "Đăng ký thành công nhưng chưa gửi được email xác thực. Hãy yêu cầu gửi lại.");
    }

    [HttpPost("verify-email"), EnableRateLimiting("auth")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken ct) =>
        Success(await auth.VerifyEmailAsync(request.Token, ct), "Xác thực email thành công. Bạn có thể đăng nhập ngay.");

    [HttpPost("resend-verification"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Resend(EmailRequest request, CancellationToken ct)
    {
        await auth.ResendVerificationAsync(request.Email, ct);
        return Success("Nếu email hợp lệ và chưa xác thực, chúng tôi đã gửi lại liên kết xác thực.");
    }

    [HttpPost("login"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct) =>
        Success(await auth.LoginAsync(request, ct), "Đăng nhập thành công.");

    [HttpPost("refresh"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken ct) =>
        Success(await auth.RefreshAsync(request.RefreshToken, ct));

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken ct)
    {
        await auth.LogoutAsync(request.RefreshToken, ct);
        return Success("Đăng xuất thành công.");
    }

    [HttpPost("forgot-password"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Forgot(EmailRequest request, CancellationToken ct)
    {
        await auth.ForgotPasswordAsync(request.Email, ct);
        return Success("Nếu email tồn tại trong hệ thống, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu.");
    }

    [HttpPost("reset-password"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Reset(ResetPasswordRequest request, CancellationToken ct)
    {
        await auth.ResetPasswordAsync(request, ct);
        return Success("Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.");
    }

    [HttpPost("change-password"), Authorize]
    public async Task<IActionResult> Change(ChangePasswordRequest request, CancellationToken ct) =>
        Success(await auth.ChangePasswordAsync(request, ct), "Đổi mật khẩu thành công. Các thiết bị khác đã bị đăng xuất.");

    [HttpGet("me"), Authorize]
    public async Task<IActionResult> Me(CancellationToken ct) => Success(await auth.GetAccountAsync(ct));
}
