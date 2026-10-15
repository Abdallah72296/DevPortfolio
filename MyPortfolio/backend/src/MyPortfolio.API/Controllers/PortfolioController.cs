using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public PortfolioController(
        IMediator mediator,
        UserManager<IdentityUser> userManager,
        IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _userManager = userManager;
        _environment = environment;
    }

    [HttpGet("cv-download")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadCv([FromQuery] string? u, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(u))
            return NotFound(new { message = "Portfolio was not found." });

        var user = await _userManager.FindByNameAsync(u.Trim());
        if (user == null)
            return NotFound(new { message = "Portfolio was not found." });

        var portfolio = await _mediator.Send(new GetPublicPortfolioQuery(user.Id), cancellationToken);
        if (!portfolio.Succeeded || portfolio.Data?.Profile is null)
            return NotFound(new { message = "Portfolio was not found." });

        var cvUrl = portfolio.Data?.Profile?.CvUrl?.Trim();
        if (string.IsNullOrWhiteSpace(cvUrl) || cvUrl.StartsWith('#'))
            return NotFound(new { message = "A CV file has not been configured for this portfolio." });

        var isLocalUrl = Url.IsLocalUrl(cvUrl);
        var isSafeAbsoluteUrl = Uri.TryCreate(cvUrl, UriKind.Absolute, out var absoluteUri) &&
            (absoluteUri.Scheme == Uri.UriSchemeHttps || absoluteUri.Scheme == Uri.UriSchemeHttp) &&
            string.IsNullOrEmpty(absoluteUri.UserInfo);
        if (!isLocalUrl && !isSafeAbsoluteUrl)
            return BadRequest(new { message = "The configured CV URL is invalid." });

        if (isLocalUrl)
        {
            if (!TryGetCvFileName(cvUrl, out var fileName))
                return BadRequest(new { message = "The configured CV file path is invalid." });

            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot)) webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var allowedBase = Path.GetFullPath(Path.Combine(webRoot, "uploads", "cv"));
            var fullPath = Path.GetFullPath(Path.Combine(allowedBase, fileName));
            var allowedPrefix = Path.EndsInDirectorySeparator(allowedBase) ? allowedBase : allowedBase + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
                return NotFound(new { message = "The configured CV file was not found." });

            var tracked = await _mediator.Send(
                new TrackPortfolioEventCommand(PortfolioEventTypes.CvDownload, user.Id), cancellationToken);
            if (!tracked.Succeeded)
                return NotFound(new { message = "Portfolio profile was not found." });

            var downloadName = !string.IsNullOrWhiteSpace(portfolio.Data?.Profile?.FullName)
                ? $"{portfolio.Data.Profile.FullName.Replace(" ", "_")}_CV.pdf"
                : "CV.pdf";
            return PhysicalFile(fullPath, "application/pdf", downloadName);
        }

        var externalTracked = await _mediator.Send(
            new TrackPortfolioEventCommand(PortfolioEventTypes.CvDownload, user.Id), cancellationToken);
        if (!externalTracked.Succeeded)
            return NotFound(new { message = "Portfolio profile was not found." });

        return Redirect(cvUrl);
    }

    private static bool TryGetCvFileName(string cvUrl, out string fileName)
    {
        fileName = string.Empty;
        var path = cvUrl.Split('?', '#')[0];
        if (path.StartsWith("~/", StringComparison.Ordinal))
            path = path[1..];

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 3 ||
            !string.Equals(segments[0], "uploads", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(segments[1], "cv", StringComparison.OrdinalIgnoreCase))
            return false;

        var decodedName = Uri.UnescapeDataString(segments[2]);
        if (decodedName.Contains('/') || decodedName.Contains('\\') ||
            !string.Equals(Path.GetFileName(decodedName), decodedName, StringComparison.Ordinal) ||
            !string.Equals(Path.GetExtension(decodedName), ".pdf", StringComparison.OrdinalIgnoreCase))
            return false;

        fileName = decodedName;
        return true;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<PortfolioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] string? u, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(u))
            return NotFound(new { message = "Portfolio username is required." });

        return await GetByUsernameCore(u, cancellationToken);
    }

    [HttpGet("{username}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<PortfolioDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetByUsername([FromRoute] string username, CancellationToken cancellationToken)
    {
        return GetByUsernameCore(username, cancellationToken);
    }

    private async Task<IActionResult> GetByUsernameCore(string username, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(username.Trim());
        if (user == null)
            return NotFound(new { message = "Portfolio was not found." });

        // Resolve the public owner from the URL username through Identity. The
        // authenticated request identity is intentionally not used here.
        var result = await _mediator.Send(new GetPublicPortfolioQuery(user.Id), cancellationToken);
        if (!result.Succeeded || result.Data?.Profile is null)
            return NotFound(new { message = "Portfolio was not found." });

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

        var result = await _mediator.Send(new GetPublicPortfolioQuery(userId, RequirePublished: false), cancellationToken);
        if (!result.Succeeded || result.Data?.Profile is null)
            return NotFound(new { message = "Portfolio profile was not found for the authenticated user." });

        return Ok(result);
    }
}
