using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriVision.Api.Services.Interfaces;

namespace NutriVision.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var result = await _dashboardService.GetDashboardSummaryAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReportSummary([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var from = fromDate ?? today.AddDays(-6);
            var to = toDate ?? today;

            var result = await _dashboardService.GetReportSummaryAsync(GetUserId(), from, to);
            return Ok(result);
        }

        [HttpGet("export-csv")]
        public async Task<IActionResult> ExportCsv([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var from = fromDate ?? today.AddDays(-30);
            var to = toDate ?? today;

            var result = await _dashboardService.ExportCsvAsync(GetUserId(), from, to);
            if (!result.Success || result.Data == null) return BadRequest(result);

            return File(result.Data, "text/csv; charset=utf-8", $"NutriVision_BaoCao_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
        }
    }
}
