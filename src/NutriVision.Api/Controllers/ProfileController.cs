using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NutriVision.Api.Common;
using NutriVision.Api.DTOs.Profile;
using NutriVision.Api.Services.Interfaces;

namespace NutriVision.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IWebHostEnvironment _env;

        public ProfileController(IProfileService profileService, IWebHostEnvironment env)
        {
            _profileService = profileService;
            _env = env;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _profileService.GetProfileAsync(GetUserId());
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var result = await _profileService.UpdateProfileAsync(GetUserId(), request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse.Fail("Vui lòng chọn ảnh đại diện."));
            }

            // BR-18 & AC-22: Giới hạn <= 5MB, jpg/png/webp
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(ApiResponse.Fail("Ảnh không được vượt quá 5MB."));
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExts.Contains(extension))
            {
                return BadRequest(ApiResponse.Fail("Chỉ hỗ trợ định dạng ảnh .jpg, .png hoặc .webp."));
            }

            var uploadsFolder = Path.Combine(_env.ContentRootPath, "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"avatar_{GetUserId()}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarUrl = $"/uploads/avatars/{fileName}";
            var result = await _profileService.UpdateAvatarAsync(GetUserId(), avatarUrl);
            return Ok(result);
        }
    }
}
