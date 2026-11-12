using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriVision.Application.Modules.Tracking;

namespace NutriVision.Api.Controllers;

[Route("api/weight"), Authorize]
public class WeightController(IWeightService weight) : ApiControllerBase
{
    /// <summary>Tổng quan + nhật ký cân nặng trong <paramref name="days"/> ngày gần nhất (7–730, mặc định 90).</summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int days = 90, CancellationToken ct = default) => Success(await weight.GetAsync(days, ct));

    [HttpPost]
    public async Task<IActionResult> Log(LogWeightRequest request, CancellationToken ct) =>
        Success(await weight.LogAsync(request, ct), "Đã ghi cân nặng.");

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await weight.DeleteAsync(id, ct);
        return Success("Đã xóa bản ghi cân nặng.");
    }
}
