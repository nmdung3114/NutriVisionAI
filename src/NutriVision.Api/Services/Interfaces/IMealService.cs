using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NutriVision.Api.Common;
using NutriVision.Api.DTOs.Meal;

namespace NutriVision.Api.Services.Interfaces
{
    public interface IMealService
    {
        Task<ApiResponse<ScanMealResponse>> UploadAndScanMealAsync(Guid userId, IFormFile file, string loaiBuaAn);
        Task<ApiResponse<MealDto>> AdjustMealAsync(Guid userId, Guid buaAnId, AdjustMealRequest request);
        Task<ApiResponse<MealDto>> ConfirmMealAsync(Guid userId, Guid buaAnId);
        Task<ApiResponse<MealDto>> RevertToAiOriginalAsync(Guid userId, Guid buaAnId);
        Task<ApiResponse<bool>> CancelMealAsync(Guid userId, Guid buaAnId);
        Task<ApiResponse<List<MealDto>>> GetMealHistoryAsync(Guid userId, DateOnly? fromDate, DateOnly? toDate, string? loaiBuaAn, int page = 1, int pageSize = 10);
        Task<ApiResponse<MealDto>> GetMealDetailAsync(Guid userId, Guid buaAnId);
        Task<ApiResponse<bool>> DeleteMealAsync(Guid userId, Guid buaAnId);
        Task<ApiResponse<MealDto>> CreateManualMealAsync(Guid userId, ManualMealRequest request);
    }
}
