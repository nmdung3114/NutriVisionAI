using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Meals;

namespace NutriVision.Api.Controllers;

[Route("api/meals"), Authorize]
public class MealsController(IMealService meals) : ApiControllerBase
{
    /// <summary>Upload ảnh + nhận diện AI → bản nháp bữa ăn.</summary>
    [HttpPost("scan"), RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Scan([FromForm] IFormFile? image, [FromForm] string? loaiBuaAn, [FromForm] DateOnly? ngayAn, CancellationToken ct)
    {
        if (image is null || image.Length == 0)
            throw ValidationAppException.Single("image", "Vui lòng chọn ảnh bữa ăn.");
        await using var stream = image.OpenReadStream();
        var meal = await meals.ScanAsync(stream, image.Length, loaiBuaAn ?? string.Empty, ngayAn, ct);
        return Success(meal, meal.AiThatBai ? "Không nhận diện được bằng AI, vui lòng nhập thủ công." : "Nhận diện thành công. Vui lòng kiểm tra kết quả.");
    }

    [HttpPost]
    public async Task<IActionResult> CreateManual(MealUpsertRequest request, CancellationToken ct)
    {
        var meal = await meals.CreateManualAsync(request, ct);
        return StatusCode(201, NutriVision.Api.Common.ApiResponse.Ok(meal, "Đã lưu bữa ăn."));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Success(await meals.GetAsync(id, ct));

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, MealUpsertRequest request, CancellationToken ct) =>
        Success(await meals.ConfirmAsync(id, request, ct), "Đã xác nhận và lưu bữa ăn.");

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, MealUpsertRequest request, CancellationToken ct) =>
        Success(await meals.UpdateAsync(id, request, ct), "Đã cập nhật bữa ăn.");

    [HttpPost("{id:guid}/revert")]
    public async Task<IActionResult> Revert(Guid id, CancellationToken ct) =>
        Success(await meals.RevertAsync(id, ct), "Đã hoàn tác về kết quả AI gốc.");

    [HttpDelete("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await meals.CancelDraftAsync(id, ct);
        return Success("Đã hủy ghi nhận bữa ăn.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await meals.DeleteAsync(id, ct);
        return Success("Đã xóa bữa ăn.");
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string? loaiBuaAn,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Success(await meals.ListAsync(new MealHistoryQuery(from, to, loaiBuaAn, page, pageSize), ct));
}
