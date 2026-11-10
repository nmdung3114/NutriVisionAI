using System;
using System.Threading.Tasks;
using NutriVision.Api.Common;
using NutriVision.Api.DTOs.Auth;

namespace NutriVision.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> RegisterAsync(RegisterRequest request);
        Task<ApiResponse<bool>> VerifyEmailAsync(string token);
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress);
        Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponse<bool>> LogoutAsync(Guid userId);
        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    }
}
