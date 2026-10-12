using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.API.Models;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Projects.Commands.CreateProject;
using MyPortfolio.Application.Projects.Commands.DeleteProject;
using MyPortfolio.Application.Projects.Commands.ReorderProjects;
using MyPortfolio.Application.Projects.Commands.SetProjectFeatured;
using MyPortfolio.Application.Projects.Commands.SetProjectVisibility;
using MyPortfolio.Application.Projects.Commands.UpdateProject;
using MyPortfolio.Application.Projects.Models;
using MyPortfolio.Application.Projects.Queries.GetProjects;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User,Guest")]
[Route("api/admin/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(
            request.Title,
            request.Description,
            request.Image,
            request.LiveUrl,
            request.GithubUrl,
            request.Technologies,
            request.DisplayOrder,
            request.IsFeatured);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProjectCommand(
            id,
            request.Title,
            request.Description,
            request.Image,
            request.LiveUrl,
            request.GithubUrl,
            request.Technologies,
            request.DisplayOrder,
            request.IsFeatured,
            request.IsVisible);

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
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisibility(
        [FromRoute] Guid id,
        [FromBody] SetProjectVisibilityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetProjectVisibilityCommand(id, request.IsVisible);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/featured")]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetFeatured(
        [FromRoute] Guid id,
        [FromBody] SetProjectFeaturedRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetProjectFeaturedCommand(id, request.IsFeatured);
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
        var command = new DeleteProjectCommand(id);
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
        [FromBody] ReorderProjectsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderProjectsCommand(request.Items);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
