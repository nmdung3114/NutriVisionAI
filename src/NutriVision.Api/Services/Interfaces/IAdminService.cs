using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NutriVision.Api.Common;
using NutriVision.Api.DTOs.Admin;

namespace NutriVision.Api.Services.Interfaces
{
    public interface IAdminService
    {
        Task<ApiResponse<List<AdminUserDto>>> GetUsersAsync(string? search, string? status, int page = 1, int pageSize = 10);
        Task<ApiResponse<bool>> ToggleUserLockAsync(Guid currentAdminId, Guid targetUserId);
        Task<ApiResponse<bool>> ChangeUserRoleAsync(Guid currentAdminId, Guid targetUserId, string newRole);
        Task<ApiResponse<List<SystemLogDto>>> GetSystemLogsAsync(string? actionType, int page = 1, int pageSize = 20);
        Task<ApiResponse<AdminStatsDto>> GetAdminStatsAsync();
    }
}
