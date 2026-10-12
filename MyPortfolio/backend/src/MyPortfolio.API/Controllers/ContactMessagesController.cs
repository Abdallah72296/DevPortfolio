using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.ContactMessages.Commands.DeleteContactMessage;
using MyPortfolio.Application.ContactMessages.Commands.MarkContactMessageAsRead;
using MyPortfolio.Application.ContactMessages.Commands.MarkContactMessageAsUnread;
using MyPortfolio.Application.ContactMessages.Models;
using MyPortfolio.Application.ContactMessages.Queries.GetContactMessageById;
using MyPortfolio.Application.ContactMessages.Queries.GetContactMessages;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User")]
[Route("api/admin/contact-messages")]
public class ContactMessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContactMessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginatedResult<ContactMessageListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isRead = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetContactMessagesQuery(page, pageSize, isRead, search);
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<ContactMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ContactMessageDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetContactMessageByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(typeof(Result<ContactMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ContactMessageDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ContactMessageDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new MarkContactMessageAsReadCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/unread")]
    [ProducesResponseType(typeof(Result<ContactMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ContactMessageDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ContactMessageDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsUnread(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new MarkContactMessageAsUnreadCommand(id);
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
        var command = new DeleteContactMessageCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return NoContent();
    }
}
