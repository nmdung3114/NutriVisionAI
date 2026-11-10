using System;
using System.Threading.Tasks;
using NutriVision.Api.Common;
using NutriVision.Api.DTOs.Profile;

namespace NutriVision.Api.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ApiResponse<ProfileDto>> GetProfileAsync(Guid userId);
        Task<ApiResponse<ProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task<ApiResponse<string>> UpdateAvatarAsync(Guid userId, string avatarUrl);
    }
}
