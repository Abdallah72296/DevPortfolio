using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.API.Models;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;
using MyPortfolio.Application.Skills.Skills.Commands.CreateSkill;
using MyPortfolio.Application.Skills.Skills.Commands.DeleteSkill;
using MyPortfolio.Application.Skills.Skills.Commands.MoveSkill;
using MyPortfolio.Application.Skills.Skills.Commands.ReorderSkills;
using MyPortfolio.Application.Skills.Skills.Commands.SetSkillVisibility;
using MyPortfolio.Application.Skills.Skills.Commands.UpdateSkill;
using MyPortfolio.Application.Skills.Skills.Queries.GetSkills;

namespace MyPortfolio.API.Controllers;

public record BatchCreateSkillsRequest(Guid SkillCategoryId, List<string> SkillNames, string? DefaultIcon = null);

[ApiController]
[Authorize(Roles = "Admin,User,Guest")]
[Route("api/admin/skills")]
public class SkillsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SkillsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<SkillDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? categoryId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSkillsQuery(categoryId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSkillCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("batch")]
    [ProducesResponseType(typeof(Result<List<SkillDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<List<SkillDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBatch(
        [FromBody] BatchCreateSkillsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MyPortfolio.Application.Skills.Skills.Commands.BatchCreateSkills.BatchCreateSkillsCommand(
            request.SkillCategoryId,
            request.SkillNames,
            request.DefaultIcon);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSkillRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSkillCommand(id, request.Name, request.Icon, request.DisplayOrder);
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
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisibility(
        [FromRoute] Guid id,
        [FromBody] SetSkillVisibilityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetSkillVisibilityCommand(id, request.IsVisible);
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
        var command = new DeleteSkillCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/category")]
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<SkillDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Move(
        [FromRoute] Guid id,
        [FromBody] MoveSkillRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MoveSkillCommand(id, request.TargetCategoryId);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("reorder")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reorder(
        [FromBody] ReorderSkillsCommand command,
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
