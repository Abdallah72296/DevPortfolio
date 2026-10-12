using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.API.Models;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Educations.Commands.CreateEducation;
using MyPortfolio.Application.Educations.Commands.DeleteEducation;
using MyPortfolio.Application.Educations.Commands.ReorderEducations;
using MyPortfolio.Application.Educations.Commands.SetEducationVisibility;
using MyPortfolio.Application.Educations.Commands.UpdateEducation;
using MyPortfolio.Application.Educations.Models;
using MyPortfolio.Application.Educations.Queries.GetEducations;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User,Guest")]
[Route("api/admin/educations")]
public class EducationController : ControllerBase
{
    private readonly IMediator _mediator;

    public EducationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<EducationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEducationsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<EducationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<EducationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEducationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateEducationCommand(
            request.InstitutionName,
            request.Degree,
            request.FieldOfStudy,
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
    [ProducesResponseType(typeof(Result<EducationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<EducationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<EducationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateEducationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEducationCommand(
            id,
            request.InstitutionName,
            request.Degree,
            request.FieldOfStudy,
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
    [ProducesResponseType(typeof(Result<EducationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<EducationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<EducationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisibility(
        [FromRoute] Guid id,
        [FromBody] SetEducationVisibilityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetEducationVisibilityCommand(id, request.IsVisible);
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
        var command = new DeleteEducationCommand(id);
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
        [FromBody] ReorderEducationsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderEducationsCommand(request.Items);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
