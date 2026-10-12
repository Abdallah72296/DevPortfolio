using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.API.Models;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.SocialLinks.Commands.CreateSocialLink;
using MyPortfolio.Application.SocialLinks.Commands.DeleteSocialLink;
using MyPortfolio.Application.SocialLinks.Commands.ReorderSocialLinks;
using MyPortfolio.Application.SocialLinks.Commands.SetSocialLinkVisibility;
using MyPortfolio.Application.SocialLinks.Commands.UpdateSocialLink;
using MyPortfolio.Application.SocialLinks.Models;
using MyPortfolio.Application.SocialLinks.Queries.GetSocialLinks;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User,Guest")]
[Route("api/admin/social-links")]
public class SocialLinksController : ControllerBase
{
    private readonly IMediator _mediator;

    public SocialLinksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<SocialLinkDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSocialLinksQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<SocialLinkDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<SocialLinkDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSocialLinkRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSocialLinkCommand(
            request.Platform,
            request.Url,
            request.Icon,
            request.DisplayOrder);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<SocialLinkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SocialLinkDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<SocialLinkDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSocialLinkRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSocialLinkCommand(
            id,
            request.Platform,
            request.Url,
            request.Icon,
            request.DisplayOrder);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/visibility")]
    [ProducesResponseType(typeof(Result<SocialLinkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SocialLinkDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<SocialLinkDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisibility(
        [FromRoute] Guid id,
        [FromBody] SetSocialLinkVisibilityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetSocialLinkVisibilityCommand(id, request.IsVisible);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSocialLinkCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return NoContent();
    }

    [HttpPut("reorder")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reorder(
        [FromBody] ReorderSocialLinksRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderSocialLinksCommand(request.Items);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
