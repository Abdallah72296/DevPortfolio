using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.API.Models;
using MyPortfolio.Application.Certifications.Commands.CreateCertification;
using MyPortfolio.Application.Certifications.Commands.DeleteCertification;
using MyPortfolio.Application.Certifications.Commands.ReorderCertifications;
using MyPortfolio.Application.Certifications.Commands.SetCertificationVisibility;
using MyPortfolio.Application.Certifications.Commands.UpdateCertification;
using MyPortfolio.Application.Certifications.Models;
using MyPortfolio.Application.Certifications.Queries.GetCertifications;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User,Guest")]
[Route("api/admin/certifications")]
public class CertificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CertificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<List<CertificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCertificationsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<CertificationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<CertificationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCertificationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCertificationCommand(
            request.Name,
            request.IssuingOrganization,
            request.IssueDate,
            request.CredentialId,
            request.CredentialUrl,
            request.Description,
            request.DisplayOrder);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<CertificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CertificationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<CertificationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCertificationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCertificationCommand(
            id,
            request.Name,
            request.IssuingOrganization,
            request.IssueDate,
            request.CredentialId,
            request.CredentialUrl,
            request.Description,
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
    [ProducesResponseType(typeof(Result<CertificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CertificationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<CertificationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisibility(
        [FromRoute] Guid id,
        [FromBody] SetCertificationVisibilityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetCertificationVisibilityCommand(id, request.IsVisible);
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
        var command = new DeleteCertificationCommand(id);
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
        [FromBody] ReorderCertificationsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderCertificationsCommand(request.Items);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
