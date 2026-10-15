using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyPortfolio.API.Models;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.ContactMessages.Commands.CreateContactMessage;
using MyPortfolio.Application.ContactMessages.Models;

namespace MyPortfolio.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/contact")]
public class ContactController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContactController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [EnableRateLimiting("public-contact")]
    [ProducesResponseType(typeof(Result<ContactMessageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<ContactMessageDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateContactMessageRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateContactMessageCommand(
            request.Name,
            request.Email,
            request.Subject,
            request.Message,
            request.Username);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
