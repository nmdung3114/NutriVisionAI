using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriVision.Application.Modules.Profile;

namespace NutriVision.Api.Controllers;

[Route("api/profile"), Authorize]
public class ProfileController(IProfileService profiles) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Success(await profiles.GetAsync(ct));

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProfileRequest request, CancellationToken ct) =>
        Success(await profiles.UpdateAsync(request, ct), "Cập nhật hồ sơ thành công.");

    [HttpPost("avatar"), RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Avatar(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw NutriVision.Application.Common.ValidationAppException.Single("file", "Vui lòng chọn ảnh đại diện.");
        await using var stream = file.OpenReadStream();
        return Success(await profiles.UpdateAvatarAsync(stream, file.Length, ct), "Đã cập nhật ảnh đại diện.");
    }
}
