using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.CV.Commands.ParseCv;
using MyPortfolio.Application.CV.Models;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User")]
[Route("api/admin/cv")]
public class CvController : ControllerBase
{
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024; // 10 MB
    private readonly IMediator _mediator;

    public CvController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("parse")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<CvPortfolioDraft>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CvPortfolioDraft>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Parse(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(Result<CvPortfolioDraft>.Failure("CV file is required."));
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(Result<CvPortfolioDraft>.Failure("Only PDF files are supported."));
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            return BadRequest(Result<CvPortfolioDraft>.Failure($"File size exceeds maximum allowed size of {MaxFileSizeInBytes / (1024 * 1024)} MB."));
        }

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        var command = new ParseCvCommand(
            memoryStream,
            file.FileName,
            file.ContentType,
            file.Length);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
