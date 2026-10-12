using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.API.Models;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Experiences.Commands.CreateExperience;
using MyPortfolio.Application.Experiences.Commands.DeleteExperience;
using MyPortfolio.Application.Experiences.Commands.ReorderExperiences;
using MyPortfolio.Application.Experiences.Commands.SetExperienceVisibility;
using MyPortfolio.Application.Experiences.Commands.UpdateExperience;
using MyPortfolio.Application.Experiences.Models;
using MyPortfolio.Application.Experiences.Queries.GetExperiences;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User,Guest")]
[Route("api/admin/experiences")]
public class ExperienceController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExperienceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<ExperienceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExperiencesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<ExperienceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<ExperienceDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExperienceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExperienceCommand(
            request.JobTitle,
            request.CompanyName,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.IsCurrent,
            request.DisplayOrder);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<ExperienceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ExperienceDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ExperienceDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateExperienceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExperienceCommand(
            id,
            request.JobTitle,
            request.CompanyName,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.IsCurrent,
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
    [ProducesResponseType(typeof(Result<ExperienceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ExperienceDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ExperienceDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisibility(
        [FromRoute] Guid id,
        [FromBody] SetExperienceVisibilityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetExperienceVisibilityCommand(id, request.IsVisible);
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
        var command = new DeleteExperienceCommand(id);
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
        [FromBody] ReorderExperiencesRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderExperiencesCommand(request.Items);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
