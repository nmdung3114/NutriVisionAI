using System;
using System.Threading.Tasks;
using NutriVision.Api.Common;
using NutriVision.Api.DTOs.Dashboard;

namespace NutriVision.Api.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync(Guid userId);
        Task<ApiResponse<ReportSummaryDto>> GetReportSummaryAsync(Guid userId, DateOnly fromDate, DateOnly toDate);
        Task<ApiResponse<byte[]>> ExportCsvAsync(Guid userId, DateOnly fromDate, DateOnly toDate);
    }
}
