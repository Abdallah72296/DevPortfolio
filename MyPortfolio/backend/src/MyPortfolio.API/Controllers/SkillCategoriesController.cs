using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.API.Models;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Categories.Commands.CreateSkillCategory;
using MyPortfolio.Application.Skills.Categories.Commands.DeleteSkillCategory;
using MyPortfolio.Application.Skills.Categories.Commands.ReorderSkillCategories;
using MyPortfolio.Application.Skills.Categories.Commands.SetSkillCategoryVisibility;
using MyPortfolio.Application.Skills.Categories.Commands.UpdateSkillCategory;
using MyPortfolio.Application.Skills.Categories.Queries.GetSkillCategories;
using MyPortfolio.Application.Skills.Models;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User,Guest")]
[Route("api/admin/skill-categories")]
public class SkillCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SkillCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<SkillCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSkillCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<SkillCategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<SkillCategoryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSkillCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("with-skills")]
    [ProducesResponseType(typeof(Result<SkillCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SkillCategoryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWithSkills(
        [FromBody] CreateCategoryWithSkillsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MyPortfolio.Application.Skills.Categories.Commands.CreateCategoryWithSkills.CreateCategoryWithSkillsCommand(
            request.Name,
            request.DisplayOrder,
            request.Skills);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<SkillCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SkillCategoryDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<SkillCategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSkillCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSkillCategoryCommand(id, request.Name, request.DisplayOrder);
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
    [ProducesResponseType(typeof(Result<SkillCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SkillCategoryDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<SkillCategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisibility(
        [FromRoute] Guid id,
        [FromBody] SetSkillCategoryVisibilityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetSkillCategoryVisibilityCommand(id, request.IsVisible);
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
        var command = new DeleteSkillCategoryCommand(id);
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
        [FromBody] ReorderSkillCategoriesCommand command,
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

public class CreateCategoryWithSkillsRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public int DisplayOrder { get; set; } = 0;
}
