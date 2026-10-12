using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Profiles.Commands.UpdateProfile;
using MyPortfolio.Application.Profiles.Models;
using MyPortfolio.Application.Profiles.Queries.GetProfile;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<ProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile([FromQuery] string? u, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(new GetProfileQuery(userId, u), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin,User,Guest")]
    [ProducesResponseType(typeof(Result<ProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        command.UserId = userId;
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
