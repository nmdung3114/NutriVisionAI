using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriVision.Application.Modules.Admin;
using NutriVision.Domain.Constants;

namespace NutriVision.Api.Controllers;

/// <summary>Khu vực quản trị: chỉ ADMIN (BR-12, BR-43). Không có endpoint nào trả dữ liệu bữa ăn/hồ sơ của người dùng.</summary>
[Route("api/admin"), Authorize(Roles = VaiTro.Admin)]
public class AdminController(IAdminService admin) : ApiControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] string? q, [FromQuery] string? trangThai, [FromQuery] string? vaiTro,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Success(await admin.ListUsersAsync(q, trangThai, vaiTro, page, pageSize, ct));

    [HttpPut("users/{id:guid}/status")]
    public async Task<IActionResult> Status(Guid id, ChangeStatusRequest request, CancellationToken ct) =>
        Success(await admin.ChangeStatusAsync(id, request, ct), request.TrangThai == TrangThaiTaiKhoan.Locked ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản.");

    [HttpPut("users/{id:guid}/role")]
    public async Task<IActionResult> Role(Guid id, ChangeRoleRequest request, CancellationToken ct) =>
        Success(await admin.ChangeRoleAsync(id, request, ct), "Đã cập nhật vai trò.");

    [HttpGet("logs")]
    public async Task<IActionResult> Logs([FromQuery] Guid? userId, [FromQuery] string? loai, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 30, CancellationToken ct = default) =>
        Success(await admin.ListLogsAsync(userId, loai, from, to, page, pageSize, ct));

    [HttpGet("logs/types")]
    public async Task<IActionResult> LogTypes(CancellationToken ct) => Success(await admin.LogTypesAsync(ct));

    [HttpGet("statistics")]
    public async Task<IActionResult> Statistics([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct) =>
        Success(await admin.StatisticsAsync(from, to, ct));
}
