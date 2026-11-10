using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriVision.Api.Services.Interfaces;

namespace NutriVision.Api.Controllers
{
    [Authorize(Roles = "ADMIN")] // BR-12, BR-43 & AC-15: Chặn User thường trả về 403 Forbidden
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        private Guid GetAdminId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _adminService.GetUsersAsync(search, status, page, pageSize);
            return Ok(result);
        }

        [HttpPut("users/{id}/toggle-lock")]
        public async Task<IActionResult> ToggleUserLock(Guid id)
        {
            var result = await _adminService.ToggleUserLockAsync(GetAdminId(), id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        public class ChangeRoleBody
        {
            public string Role { get; set; } = "USER";
        }

        [HttpPut("users/{id}/change-role")]
        public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] ChangeRoleBody body)
        {
            var result = await _adminService.ChangeUserRoleAsync(GetAdminId(), id, body.Role);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetSystemLogs(
            [FromQuery] string? actionType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _adminService.GetSystemLogsAsync(actionType, page, pageSize);
            return Ok(result);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _adminService.GetAdminStatsAsync();
            return Ok(result);
        }
    }
}
