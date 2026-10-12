using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.SiteSettings.Commands.PublishSiteSettings;
using MyPortfolio.Application.SiteSettings.Commands.UpdateSiteSettings;
using MyPortfolio.Application.SiteSettings.Models;
using MyPortfolio.Application.SiteSettings.Queries.GetSiteSettings;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Route("api/admin/site-settings")]
[Authorize(Roles = "Admin,User")]
public sealed class SiteSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SiteSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<PortfolioSiteSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSiteSettingsQuery(CreateIfNotExists: true), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(typeof(Result<PortfolioSiteSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PortfolioSiteSettingsDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] UpdateSiteSettingsRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateSiteSettingsCommand(
            request.ThemeKey,
            request.CustomDomain,
            request.SeoTitle,
            request.SeoDescription,
            request.SeoKeywords);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("publish")]
    [ProducesResponseType(typeof(Result<PublishSiteSettingsResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Publish(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PublishSiteSettingsCommand(), cancellationToken);
        return Ok(result);
    }
}

[ApiController]
[AllowAnonymous]
[Route("api/portfolio/site-settings")]
public sealed class PublicSiteSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicSiteSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<PortfolioSiteSettingsDto?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSiteSettingsQuery(CreateIfNotExists: false), cancellationToken);
        return Ok(result);
    }
}

public sealed record UpdateSiteSettingsRequest(
    string ThemeKey,
    string? CustomDomain,
    string? SeoTitle,
    string? SeoDescription,
    string? SeoKeywords);
