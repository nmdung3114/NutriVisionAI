using Microsoft.AspNetCore.Mvc;
using NutriVision.Application.Abstractions;

namespace NutriVision.Api.Controllers;

/// <summary>Phục vụ ảnh qua URL ký có hạn (chữ ký chỉ được cấp cho chủ sở hữu khi trả DTO).</summary>
[ApiController, Route("api/files")]
public class FilesController(IFileStorage storage, IFileUrlSigner signer) : ControllerBase
{
    [HttpGet("{**path}")]
    public async Task<IActionResult> Get(string path, [FromQuery] long exp, [FromQuery] string? sig, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(sig) || path.Contains("..") || !signer.Verify(path, exp, sig)) return NotFound();

        var stream = await storage.OpenReadAsync(path, ct);
        if (stream is null) return NotFound();

        var contentType = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };
        Response.Headers["Cache-Control"] = "private, max-age=1800";
        Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin";
        return File(stream, contentType);
    }
}
