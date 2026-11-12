using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriVision.Application.Modules.Foods;

namespace NutriVision.Api.Controllers;

[Route("api/foods"), Authorize]
public class FoodsController(IFoodService foods) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] string? nhom, [FromQuery] int take = 20, CancellationToken ct = default) =>
        Success(await foods.SearchAsync(q, nhom, take, ct));

    [HttpGet("match")]
    public async Task<IActionResult> Match([FromQuery] string name, CancellationToken ct) =>
        Success(await foods.MatchAsync(name ?? string.Empty, ct));

    [HttpGet("units")]
    public IActionResult Units() => Success(foods.Units());

    [HttpGet("groups")]
    public async Task<IActionResult> Groups(CancellationToken ct) => Success(await foods.GroupsAsync(ct));
}
