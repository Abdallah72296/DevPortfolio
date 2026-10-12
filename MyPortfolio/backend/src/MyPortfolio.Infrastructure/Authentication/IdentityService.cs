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
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService,
        IApplicationDbContext dbContext,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
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
            _logger.LogWarning("Login failed: User '{Input}' not found.", input);
            return Result<AuthResponseDto>.Failure("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed: Incorrect password for user '{Input}'.", input);
            return Result<AuthResponseDto>.Failure("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _tokenService.GenerateToken(user, roles);

        _logger.LogInformation("User '{Email}' authenticated successfully.", user.Email);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            Email = user.Email ?? user.UserName ?? input,
            Roles = roles.ToArray()
        }, "Login successful.");
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(
        string fullName,
        string username,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result<AuthResponseDto>.Failure("Username is required.");
        if (string.IsNullOrWhiteSpace(email))
            return Result<AuthResponseDto>.Failure("Email is required.");
        if (string.IsNullOrWhiteSpace(password))
            return Result<AuthResponseDto>.Failure("Password is required.");

        username = username.Trim().ToLowerInvariant().Replace(" ", "_");
        email = email.Trim().ToLowerInvariant();

        var existingUserByEmail = await _userManager.FindByEmailAsync(email);
        if (existingUserByEmail != null)
        {
            return Result<AuthResponseDto>.Failure("An account with this email already exists.");
        }

        var existingUserByName = await _userManager.FindByNameAsync(username);
        if (existingUserByName != null)
        {
            return Result<AuthResponseDto>.Failure("Username is already taken. Please choose another username.");
        }

        var user = new IdentityUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result<AuthResponseDto>.Failure($"Registration failed: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Result<AuthResponseDto>.Failure("Registration failed: could not assign the User role.");
        }

        var cleanName = !string.IsNullOrWhiteSpace(fullName) ? fullName.Trim() : username;
        var profile = new MyPortfolio.Domain.Entities.UserProfile(
            userId: user.Id,
            fullName: cleanName,
            title: "Professional Portfolio",
            headlineRole: "Professional Portfolio",
            badge: "PORTFOLIO",
            location: string.Empty,
            email: email,
            phone: string.Empty,
            summary: $"{cleanName}'s professional portfolio.",
            longBio: string.Empty,
            avatarUrl: "",
            cvUrl: "",
            yearsOfExperience: string.Empty,
            paragraphsJson: "[]",
            availableForHire: false,
            cardCaption: string.Empty
        );
        profile.AssignOwner(user.Id);

        await _dbContext.UserProfiles.AddAsync(profile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _tokenService.GenerateToken(user, roles);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            Email = user.Email ?? email,
            Roles = roles.ToArray()
        }, "Account created successfully.");
    }
}
