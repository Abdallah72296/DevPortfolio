using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Application.Analytics.Commands.TrackPortfolioEvent;
using MyPortfolio.Application.Analytics.Queries.GetPortfolioAnalyticsSummary;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    [AllowAnonymous]
    [HttpPost("events")]
    public async Task<IActionResult> Track(
        [FromBody] TrackPortfolioEventRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new TrackPortfolioEventCommand(request.EventType, request.Username), cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result);

        return Accepted(result);
    }

    [Authorize(Roles = "Admin,User,Guest")]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfolioAnalyticsSummaryQuery(), cancellationToken);
        return Ok(result);
    }
}

public record TrackPortfolioEventRequest(string EventType, string? Username = null);
