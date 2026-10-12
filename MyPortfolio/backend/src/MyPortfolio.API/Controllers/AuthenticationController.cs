using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Application.Authentication.Commands.Login;
using MyPortfolio.Application.Authentication.Models;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthenticationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            // If the failure is from credential verification, return 401 Unauthorized
            if (result.Message?.Contains("Invalid email or password", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Unauthorized(result);
            }

            // Otherwise, it is a validation failure (e.g. malformed email or missing field)
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] MyPortfolio.Application.Common.Interfaces.IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        var result = await identityService.RegisterAsync(
            request.FullName,
            request.Username,
            request.Email,
            request.Password,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
