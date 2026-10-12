using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.API.Models;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.PortfolioSections.Commands.CreatePortfolioSection;
using MyPortfolio.Application.PortfolioSections.Commands.DeletePortfolioSection;
using MyPortfolio.Application.PortfolioSections.Commands.ReorderPortfolioSections;
using MyPortfolio.Application.PortfolioSections.Commands.SetPortfolioSectionVisibility;
using MyPortfolio.Application.PortfolioSections.Commands.UpdatePortfolioSection;
using MyPortfolio.Application.PortfolioSections.Models;
using MyPortfolio.Application.PortfolioSections.Queries.GetPortfolioSections;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User,Guest")]
[Route("api/admin/portfolio-sections")]
public class PortfolioSectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PortfolioSectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<PortfolioSectionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfolioSectionsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<PortfolioSectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<PortfolioSectionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePortfolioSectionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<PortfolioSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PortfolioSectionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<PortfolioSectionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdatePortfolioSectionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePortfolioSectionCommand(id, request.Title, request.DisplayOrder);
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
    [ProducesResponseType(typeof(Result<PortfolioSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PortfolioSectionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<PortfolioSectionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisibility(
        [FromRoute] Guid id,
        [FromBody] SetPortfolioSectionVisibilityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetPortfolioSectionVisibilityCommand(id, request.IsVisible);
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
        var command = new DeletePortfolioSectionCommand(id);
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
        [FromBody] ReorderPortfolioSectionsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
