using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyPortfolio.Application.Authentication.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Infrastructure.Authentication;

public class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService,
        IEmailConfirmationService emailConfirmationService,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailConfirmationService = emailConfirmationService;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var input = email?.Trim() ?? string.Empty;
        var user = await _userManager.FindByEmailAsync(input)
                   ?? await _userManager.FindByNameAsync(input);

        if (user == null)
        {
            _logger.LogWarning("Login failed because the supplied account was not found.");
            return Result<AuthResponseDto>.Failure("Invalid email or password.");
        }

        if (await _userManager.IsLockedOutAsync(user))
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            await _userManager.AccessFailedAsync(user);
            _logger.LogWarning("Login failed because the supplied password was incorrect.");
            return Result<AuthResponseDto>.Failure("Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        if (!user.EmailConfirmed)
        {
            return Result<AuthResponseDto>.Failure("Please confirm your email address before signing in.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _tokenService.GenerateToken(user, roles);

        _logger.LogInformation("User {UserId} authenticated successfully.", user.Id);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            Email = user.Email ?? user.UserName ?? input,
            Roles = roles.ToArray()
        }, "Login successful.");
    }

    public async Task<Result<RegistrationResponseDto>> RegisterAsync(
        string fullName,
        string username,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        return await _emailConfirmationService.StartRegistrationAsync(fullName, username, email, password, cancellationToken);
    }

}
