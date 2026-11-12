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

[Route("api/water"), Authorize]
public class WaterController(IWaterService water) : ApiControllerBase
{
    /// <summary>Lượng nước của ngày (mặc định hôm nay), mục tiêu theo cân nặng và 7 ngày gần nhất.</summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly? ngay, CancellationToken ct) => Success(await water.GetAsync(ngay, ct));

    /// <summary>Cộng thêm (ml &gt; 0) hoặc hoàn tác (ml &lt; 0) lượng nước.</summary>
    [HttpPost]
    public async Task<IActionResult> Add(AddWaterRequest request, CancellationToken ct) => Success(await water.AddAsync(request, ct));
}

