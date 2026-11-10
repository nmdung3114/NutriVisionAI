using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NutriVision.Api.DTOs.Meal;
using NutriVision.Api.Services.Interfaces;

namespace NutriVision.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MealController : ControllerBase
    {
        private readonly IMealService _mealService;

        public MealController(IMealService mealService)
        {
            _mealService = mealService;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("upload-scan")]
        public async Task<IActionResult> UploadAndScanMeal([FromForm] IFormFile file, [FromForm] string loaiBuaAn)
        {
            var result = await _mealService.UploadAndScanMealAsync(GetUserId(), file, loaiBuaAn);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}/adjust")]
        public async Task<IActionResult> AdjustMeal(Guid id, [FromBody] AdjustMealRequest request)
        {
            var result = await _mealService.AdjustMealAsync(GetUserId(), id, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmMeal(Guid id)
        {
            var result = await _mealService.ConfirmMealAsync(GetUserId(), id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{id}/revert")]
        public async Task<IActionResult> RevertToAiOriginal(Guid id)
        {
            var result = await _mealService.RevertToAiOriginalAsync(GetUserId(), id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}/cancel")]
        public async Task<IActionResult> CancelMeal(Guid id)
        {
            var result = await _mealService.CancelMealAsync(GetUserId(), id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetMealHistory(
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate,
            [FromQuery] string? loaiBuaAn,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _mealService.GetMealHistoryAsync(GetUserId(), fromDate, toDate, loaiBuaAn, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMealDetail(Guid id)
        {
            var result = await _mealService.GetMealDetailAsync(GetUserId(), id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeal(Guid id)
        {
            var result = await _mealService.DeleteMealAsync(GetUserId(), id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("manual")]
        public async Task<IActionResult> CreateManualMeal([FromBody] ManualMealRequest request)
        {
            var result = await _mealService.CreateManualMealAsync(GetUserId(), request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
