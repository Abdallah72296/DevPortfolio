using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Analytics.Commands.TrackPortfolioEvent;
using MyPortfolio.Application.Portfolio.Models;
using MyPortfolio.Application.Portfolio.Queries.GetPublicPortfolio;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Route("api/portfolio")]
public class PortfolioController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public PortfolioController(IMediator mediator, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment;
    }

    [HttpGet("cv-download")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadCv([FromQuery] string? u, CancellationToken cancellationToken)
    {
        var portfolio = await _mediator.Send(new GetPublicPortfolioQuery(u), cancellationToken);
        var cvUrl = portfolio.Data?.Profile?.CvUrl?.Trim();
        if (string.IsNullOrWhiteSpace(cvUrl) || cvUrl.StartsWith('#'))
            return NotFound(new { message = "A CV file has not been configured for this portfolio." });

        var isLocalUrl = Url.IsLocalUrl(cvUrl) || cvUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase);
        var isSafeAbsoluteUrl = Uri.TryCreate(cvUrl, UriKind.Absolute, out var absoluteUri) &&
            (absoluteUri.Scheme == Uri.UriSchemeHttps || absoluteUri.Scheme == Uri.UriSchemeHttp);
        if (!isLocalUrl && !isSafeAbsoluteUrl)
            return BadRequest(new { message = "The configured CV URL is invalid." });

        var tracked = await _mediator.Send(
            new TrackPortfolioEventCommand(PortfolioEventTypes.CvDownload, u), cancellationToken);
        if (!tracked.Succeeded)
            return NotFound(new { message = "Portfolio profile was not found." });

        if (isLocalUrl)
        {
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot)) webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var relativePath = cvUrl.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(webRoot, relativePath));
            var allowedBase = Path.GetFullPath(webRoot);

            if (fullPath.StartsWith(allowedBase, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
            {
                var downloadName = !string.IsNullOrWhiteSpace(portfolio.Data?.Profile?.FullName)
                    ? $"{portfolio.Data.Profile.FullName.Replace(" ", "_")}_CV.pdf"
                    : "CV.pdf";

                return PhysicalFile(fullPath, "application/pdf", downloadName);
            }
        }

        return Redirect(cvUrl);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<PortfolioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] string? u, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPublicPortfolioQuery(u), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{username}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<PortfolioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUsername([FromRoute] string username, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPublicPortfolioQuery(username), cancellationToken);
        return Ok(result);
    }

    [HttpGet("mobile-preview")]
    [Authorize]
    [ProducesResponseType(typeof(Result<PortfolioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMobilePreview(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetPublicPortfolioQuery(userId), cancellationToken);
        if (!result.Succeeded || result.Data?.Profile is null)
            return NotFound(new { message = "Portfolio profile was not found for the authenticated user." });

        return Ok(result);
    }
}
