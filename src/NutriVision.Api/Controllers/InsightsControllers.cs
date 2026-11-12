using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriVision.Application.Modules.Dashboard;
using NutriVision.Application.Modules.Habits;
using NutriVision.Application.Modules.Reports;
using NutriVision.Application.Modules.Suggestions;

namespace NutriVision.Api.Controllers;

[Route("api/dashboard"), Authorize]
public class DashboardController(IDashboardService dashboard, IHabitService habits) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Success(await dashboard.GetAsync(ct));

    /// <summary>Lịch nhiệt tháng (mặc định tháng hiện tại).</summary>
    [HttpGet("calendar")]
    public async Task<IActionResult> Calendar([FromQuery] int? nam, [FromQuery] int? thang, CancellationToken ct) =>
        Success(await habits.GetCalendarAsync(nam, thang, ct));
}

[Route("api/suggestions"), Authorize]
public class SuggestionsController(ISuggestionService suggestions) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Success(await suggestions.GetAsync(ct));
}

[Route("api/reports"), Authorize]
public class ReportsController(IReportService reports) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct) =>
        Success(await reports.GetAsync(from, to, ct));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string format = "pdf", CancellationToken ct = default)
    {
        var fmt = format.Equals("csv", StringComparison.OrdinalIgnoreCase) ? ExportFormat.Csv
            : format.Equals("pdf", StringComparison.OrdinalIgnoreCase) ? ExportFormat.Pdf
            : throw NutriVision.Application.Common.ValidationAppException.Single("format", "Định dạng chỉ có thể là pdf hoặc csv.");
        var file = await reports.ExportAsync(from, to, fmt, ct);
        Response.Headers["Access-Control-Expose-Headers"] = "Content-Disposition";
        return File(file.Content, file.ContentType, file.FileName);
    }
}
