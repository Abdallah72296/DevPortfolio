using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyPortfolio.Application.Analytics.Commands.TrackPortfolioEvent;
using MyPortfolio.Application.Analytics.Queries.GetPortfolioAnalyticsSummary;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<IdentityUser> _userManager;

    public AnalyticsController(IMediator mediator, UserManager<IdentityUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpPost("events")]
    [EnableRateLimiting("analytics-events")]
    public async Task<IActionResult> Track(
        [FromBody] TrackPortfolioEventRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return NotFound(new { message = "Portfolio username is required." });

        var user = await _userManager.FindByNameAsync(request.Username.Trim());
        if (user == null)
            return NotFound(new { message = "Portfolio was not found." });

        var result = await _mediator.Send(
            new TrackPortfolioEventCommand(request.EventType, user.Id), cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result);

        return Accepted(result);
    }

    [Authorize(Roles = "Admin,User,Guest")]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(ownerUserId))
            return Unauthorized();

        var result = await _mediator.Send(new GetPortfolioAnalyticsSummaryQuery(ownerUserId), cancellationToken);
        return Ok(result);
    }
}

public record TrackPortfolioEventRequest(string EventType, string? Username = null);
