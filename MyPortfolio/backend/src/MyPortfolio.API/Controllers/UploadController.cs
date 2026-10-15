using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Application.Common.Interfaces;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;

    public UploadController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpPost("image")]
    [Authorize(Roles = "Admin,User,Guest")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage([FromForm] UploadFileDto dto, CancellationToken cancellationToken)
    {
        if (dto?.File == null || dto.File.Length == 0)
        {
            return BadRequest(new { succeeded = false, message = "No file was uploaded." });
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        await using var stream = dto.File.OpenReadStream();

        var result = await _fileUploadService.UploadImageAsync(
            stream,
            dto.File.FileName,
            dto.File.ContentType,
            dto.File.Length,
            baseUrl,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new { succeeded = false, message = result.Message });
        }

        return Ok(new
        {
            succeeded = true,
            url = result.Url,
            relativeUrl = result.RelativeUrl,
            fileName = result.FileName
        });
    }

    [HttpPost("cv")]
    [Authorize(Roles = "Admin,User")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadCv([FromForm] UploadFileDto dto, CancellationToken cancellationToken)
    {
        if (dto?.File == null || dto.File.Length == 0)
        {
            return BadRequest(new { succeeded = false, message = "A CV PDF file is required." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { succeeded = false, message = "A valid user identity is required to upload a CV." });

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        await using var stream = dto.File.OpenReadStream();

        var result = await _fileUploadService.UploadCvAsync(
            stream,
            dto.File.FileName,
            dto.File.ContentType,
            dto.File.Length,
            userId,
            baseUrl,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new { succeeded = false, message = result.Message });
        }

        return Ok(new
        {
            succeeded = true,
            url = result.Url,
            relativeUrl = result.RelativeUrl,
            fileName = result.FileName
        });
    }
}

public class UploadFileDto
{
    public IFormFile? File { get; set; }
}
