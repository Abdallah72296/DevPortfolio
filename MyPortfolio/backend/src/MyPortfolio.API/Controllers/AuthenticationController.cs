using MediatR;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyPortfolio.Application.Authentication.Commands.Login;
using MyPortfolio.Application.Authentication.Commands.Register;
using MyPortfolio.Application.Authentication.Commands.ConfirmEmail;
using MyPortfolio.Application.Authentication.Commands.ConfirmEmailOtp;
using MyPortfolio.Application.Authentication.Commands.ResendConfirmationOtp;
using MyPortfolio.Application.Authentication.Models;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IMediator mediator, ILogger<AuthenticationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.Succeeded)
            {
                if (result.Message?.Contains("Invalid email or password", StringComparison.OrdinalIgnoreCase) == true)
                    return Unauthorized(result);

                if (result.Message?.Contains("confirm your email", StringComparison.OrdinalIgnoreCase) == true)
                    return StatusCode(StatusCodes.Status403Forbidden, result);

                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (DbException exception)
        {
            _logger.LogError(exception, "Login is unavailable because the database could not be reached.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                Result<AuthResponseDto>.Failure("Sign-in is temporarily unavailable because the server cannot reach its database. Please try again later."));
        }
        catch (TimeoutException exception)
        {
            _logger.LogError(exception, "Login request timed out while accessing the database.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                Result<AuthResponseDto>.Failure("Sign-in is temporarily unavailable. Please try again later."));
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<MyPortfolio.Application.Authentication.Models.RegistrationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<MyPortfolio.Application.Authentication.Models.RegistrationResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RegisterCommand(request.FullName, request.Username, request.Email, request.Password),
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [EnableRateLimiting("email-confirmation")]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmEmailCommand(request.Email, request.Token), cancellationToken);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("confirm-email-otp")]
    [AllowAnonymous]
    [EnableRateLimiting("email-confirmation")]
    public async Task<IActionResult> ConfirmEmailOtp(
        [FromBody] ConfirmEmailOtpRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmEmailOtpCommand(request.Email, request.Code), cancellationToken);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    [EnableRateLimiting("email-resend")]
    public async Task<IActionResult> ResendConfirmation(
        [FromBody] ResendConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ResendConfirmationOtpCommand(request.Email), cancellationToken);
        return result.Succeeded ? Ok(result) : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
    }
}

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed record ConfirmEmailRequest(string Email, string Token);
public sealed record ConfirmEmailOtpRequest(string Email, string Code);
public sealed record ResendConfirmationRequest(string Email);
