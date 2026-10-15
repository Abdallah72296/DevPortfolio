using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPortfolio.Application.Authentication.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Domain.Entities;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Infrastructure.Authentication;

public sealed class EmailConfirmationService : IEmailConfirmationService
{
    private const string OtpTokenProvider = "DevFolio";
    private const string OtpTokenName = "EmailConfirmationOtp";
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PendingRegistrationLifetime = TimeSpan.FromHours(24);
    private const int MaxOtpAttempts = 5;

    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailConfirmationService> _logger;

    public EmailConfirmationService(
        UserManager<IdentityUser> userManager,
        ApplicationDbContext dbContext,
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<EmailConfirmationService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<RegistrationResponseDto>> StartRegistrationAsync(
        string fullName, string username, string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return Result<RegistrationResponseDto>.Failure("Username is required.");
        if (string.IsNullOrWhiteSpace(email)) return Result<RegistrationResponseDto>.Failure("Email is required.");
        if (string.IsNullOrWhiteSpace(password)) return Result<RegistrationResponseDto>.Failure("Password is required.");

        username = username.Trim().ToLowerInvariant().Replace(" ", "_");
        email = email.Trim().ToLowerInvariant();
        var existingEmailUser = await _userManager.FindByEmailAsync(email);
        if (existingEmailUser is not null)
            return Result<RegistrationResponseDto>.Failure("An account with this email already exists.");
        var existingNameUser = await _userManager.FindByNameAsync(username);
        if (existingNameUser is not null)
            return Result<RegistrationResponseDto>.Failure("Username is already taken. Please choose another username.");

        var userForValidation = new IdentityUser { UserName = username, Email = email };
        foreach (var validator in _userManager.PasswordValidators)
        {
            var validation = await validator.ValidateAsync(_userManager, userForValidation, password);
            if (!validation.Succeeded)
            {
                var errors = string.Join("; ", validation.Errors.Select(error => error.Description));
                return Result<RegistrationResponseDto>.Failure($"Registration failed: {errors}");
            }
        }

        var normalizedEmail = _userManager.NormalizeEmail(email) ?? email.ToUpperInvariant();
        var normalizedUsername = _userManager.NormalizeName(username) ?? username.ToUpperInvariant();
        var pendingRegistrationCutoff = DateTimeOffset.UtcNow.Subtract(PendingRegistrationLifetime);
        await _dbContext.PendingRegistrations
            .Where(item => item.CreatedAt <= pendingRegistrationCutoff)
            .ExecuteDeleteAsync(cancellationToken);
        var pendingByEmail = await _dbContext.PendingRegistrations
            .SingleOrDefaultAsync(item => item.NormalizedEmail == normalizedEmail, cancellationToken);
        var pendingByName = await _dbContext.PendingRegistrations
            .SingleOrDefaultAsync(item => item.NormalizedUsername == normalizedUsername, cancellationToken);
        if (pendingByName is not null && pendingByName.Id != pendingByEmail?.Id)
            return Result<RegistrationResponseDto>.Failure("Username is already reserved by a pending registration.");

        var pending = pendingByEmail ?? new PendingRegistration();
        pending.Email = email;
        pending.NormalizedEmail = normalizedEmail;
        pending.Username = username;
        pending.NormalizedUsername = normalizedUsername;
        pending.FullName = string.IsNullOrWhiteSpace(fullName) ? username : fullName.Trim();
        pending.PasswordHash = _userManager.PasswordHasher.HashPassword(userForValidation, password);
        pending.CreatedAt = DateTimeOffset.UtcNow;
        var code = SetNewOtp(pending);

        if (pendingByEmail is null)
            await _dbContext.PendingRegistrations.AddAsync(pending, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var sent = await SendOtpEmailAsync(pending.Email, pending.FullName, code, pending.Id.ToString(), cancellationToken);
        return Result<RegistrationResponseDto>.Success(
            new RegistrationResponseDto(email, sent),
            sent
                ? "Registration is pending. Enter the six-digit code sent to your email to create your account."
                : "Registration is pending, but the confirmation email could not be sent. Check SMTP settings and request a new code.");
    }

    public async Task<bool> SendOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null || user.EmailConfirmed || string.IsNullOrWhiteSpace(user.Email)) return false;
        try
        {
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var expiresAt = DateTimeOffset.UtcNow.Add(OtpLifetime).ToUnixTimeSeconds();
            var hash = Convert.ToHexString(ComputeOtpHash(user.Id, code));
            var tokenResult = await _userManager.SetAuthenticationTokenAsync(user, OtpTokenProvider, OtpTokenName, $"{expiresAt}:0:{hash}");
            if (!tokenResult.Succeeded) return false;
            await SendOtpEmailAsync(user.Email, user.UserName ?? user.Email, code, user.Id, cancellationToken);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Could not send email confirmation code for user {UserId}.", user.Id);
            return false;
        }
    }

    public async Task<Result> ConfirmLinkAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return Result.Failure("The confirmation link is invalid or incomplete.");
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null) return Result.Failure("The confirmation link is invalid or expired.");
        if (user.EmailConfirmed) return Result.Success("Email address is already confirmed.");
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded) return Result.Failure("The confirmation link is invalid or expired.");
        await _userManager.RemoveAuthenticationTokenAsync(user, OtpTokenProvider, OtpTokenName);
        return Result.Success("Email address confirmed. You can now sign in.");
    }

    public async Task<Result> ConfirmOtpAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) || code.Length != 6 || code.Any(character => !char.IsAsciiDigit(character)))
            return Result.Failure("Enter the six-digit code from your email.");

        var normalizedEmail = _userManager.NormalizeEmail(email.Trim());
        var pending = normalizedEmail is null ? null : await _dbContext.PendingRegistrations
            .SingleOrDefaultAsync(item => item.NormalizedEmail == normalizedEmail, cancellationToken);
        if (pending is not null)
            return await ConfirmPendingRegistrationAsync(pending, code, cancellationToken);

        // Compatibility for accounts created before pending signup was introduced.
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null || user.EmailConfirmed)
            return Result.Failure("The code is invalid or expired. Request a new code and try again.");
        var stored = await _userManager.GetAuthenticationTokenAsync(user, OtpTokenProvider, OtpTokenName);
        var parts = stored?.Split(':');
        if (!TryReadOtp(parts, out var expiresAt, out _, out var expectedHash))
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, OtpTokenProvider, OtpTokenName);
            return Result.Failure("The code is invalid or expired. Request a new code and try again.");
        }
        if (!VerifyOtp(user.Id, code, expectedHash))
        {
            return await RecordLegacyOtpFailureAsync(user.Id, expiresAt, expectedHash, cancellationToken);
        }
        user.EmailConfirmed = true;
        var confirmation = await _userManager.UpdateAsync(user);
        if (!confirmation.Succeeded) return Result.Failure("Email confirmation could not be completed. Request a new code and try again.");
        await _userManager.RemoveAuthenticationTokenAsync(user, OtpTokenProvider, OtpTokenName);
        return Result.Success("Email address confirmed. You can now sign in.");
    }

    public async Task<Result> ResendOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        if (!IsEmailDeliveryConfigured())
            return Result.Failure("Email confirmation is not available because email delivery is not configured.");
        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = _userManager.NormalizeEmail(email.Trim());
            var pending = normalizedEmail is null ? null : await _dbContext.PendingRegistrations
                .SingleOrDefaultAsync(item => item.NormalizedEmail == normalizedEmail, cancellationToken);
            if (pending is not null && pending.CreatedAt.Add(PendingRegistrationLifetime) > DateTimeOffset.UtcNow)
            {
                var code = SetNewOtp(pending);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await SendOtpEmailAsync(pending.Email, pending.FullName, code, pending.Id.ToString(), cancellationToken);
            }
            else
            {
                await SendOtpAsync(email.Trim(), cancellationToken);
            }
        }
        return Result.Success("If the account exists and still needs confirmation, a new code will be sent.");
    }

    private bool IsEmailDeliveryConfigured() =>
        (!string.IsNullOrWhiteSpace(_configuration["Email:Resend:ApiKey"]) &&
         !string.IsNullOrWhiteSpace(_configuration["Email:Resend:From"])) ||
        (!string.IsNullOrWhiteSpace(_configuration["Email:Smtp:Host"]) &&
         !string.IsNullOrWhiteSpace(_configuration["Email:Smtp:From"]));

    private async Task<Result> RecordLegacyOtpFailureAsync(
        string userId,
        long expiresAt,
        string expectedHash,
        CancellationToken cancellationToken)
    {
        for (var retry = 0; retry < 10; retry++)
        {
            var currentValue = await _dbContext.UserTokens
                .AsNoTracking()
                .Where(token => token.UserId == userId && token.LoginProvider == OtpTokenProvider && token.Name == OtpTokenName)
                .Select(token => token.Value)
                .SingleOrDefaultAsync(cancellationToken);
            var parts = currentValue?.Split(':');
            if (!TryReadOtp(parts, out var currentExpiry, out var attempts, out var currentHash) ||
                currentExpiry != expiresAt || !string.Equals(currentHash, expectedHash, StringComparison.Ordinal))
                return Result.Failure("The code is invalid or expired. Request a new code and try again.");

            var nextAttempt = attempts + 1;
            var tokenQuery = _dbContext.UserTokens.Where(token => token.UserId == userId &&
                token.LoginProvider == OtpTokenProvider && token.Name == OtpTokenName && token.Value == currentValue);

            var affected = nextAttempt >= MaxOtpAttempts
                ? await tokenQuery.ExecuteDeleteAsync(cancellationToken)
                : await tokenQuery.ExecuteUpdateAsync(update => update
                    .SetProperty(token => token.Value, $"{expiresAt}:{nextAttempt}:{expectedHash}"), cancellationToken);

            if (affected == 1)
                return Result.Failure(nextAttempt >= MaxOtpAttempts
                    ? "Too many incorrect attempts. Request a new code."
                    : "The code is incorrect. Check the email and try again.");
        }

        return Result.Failure("Verification is busy. Please try again.");
    }

    private async Task<Result> ConfirmPendingRegistrationAsync(PendingRegistration pending, string code, CancellationToken cancellationToken)
    {
        if (pending.CreatedAt.Add(PendingRegistrationLifetime) <= DateTimeOffset.UtcNow ||
            pending.OtpExpiresAt <= DateTimeOffset.UtcNow || pending.OtpAttempts >= MaxOtpAttempts || string.IsNullOrEmpty(pending.OtpHash))
            return Result.Failure("The code is invalid or expired. Request a new code and try again.");

        if (!VerifyOtp(pending.Id.ToString(), code, pending.OtpHash))
        {
            return await RecordPendingOtpFailureAsync(pending.Id, pending.OtpHash, cancellationToken);
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var user = new IdentityUser
            {
                UserName = pending.Username,
                NormalizedUserName = pending.NormalizedUsername,
                Email = pending.Email,
                NormalizedEmail = pending.NormalizedEmail,
                EmailConfirmed = true,
                PasswordHash = pending.PasswordHash,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return Result.Failure("The account could not be finalized. Request a new code and try again.");

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
                return Result.Failure("The account could not be finalized. Please contact support.");

            var fullName = pending.FullName;
            var profile = new UserProfile(
                userId: user.Id,
                fullName: fullName,
                title: "Professional Portfolio",
                headlineRole: "Professional Portfolio",
                badge: "PORTFOLIO",
                location: string.Empty,
                email: pending.Email,
                phone: string.Empty,
                summary: $"{fullName}'s professional portfolio.",
                longBio: string.Empty,
                avatarUrl: string.Empty,
                cvUrl: string.Empty,
                yearsOfExperience: string.Empty,
                paragraphsJson: "[]",
                availableForHire: false,
                cardCaption: string.Empty);
            profile.AssignOwner(user.Id);
            await _dbContext.UserProfiles.AddAsync(profile, cancellationToken);
            _dbContext.PendingRegistrations.Remove(pending);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Pending registration verified and account created for user {UserId}.", user.Id);
            return Result.Success("Email verified and account created. You can now sign in.");
        });
    }

    private async Task<Result> RecordPendingOtpFailureAsync(Guid registrationId, string expectedOtpHash, CancellationToken cancellationToken)
    {
        // Compare-and-set makes every concurrent failed attempt increment the counter
        // exactly once; stale EF entities can no longer overwrite another request's count.
        for (var retry = 0; retry < 10; retry++)
        {
            var current = await _dbContext.PendingRegistrations
                .AsNoTracking()
                .Where(item => item.Id == registrationId)
                .Select(item => new { item.OtpAttempts, item.OtpHash, item.OtpExpiresAt, item.CreatedAt })
                .SingleOrDefaultAsync(cancellationToken);

            var now = DateTimeOffset.UtcNow;
            if (current is null || !string.Equals(current.OtpHash, expectedOtpHash, StringComparison.Ordinal) ||
                current.CreatedAt.Add(PendingRegistrationLifetime) <= now ||
                current.OtpExpiresAt <= now || current.OtpAttempts >= MaxOtpAttempts ||
                string.IsNullOrEmpty(current.OtpHash))
                return Result.Failure("The code is invalid or expired. Request a new code and try again.");

            var nextAttempt = current.OtpAttempts + 1;
            var affected = await _dbContext.PendingRegistrations
                .Where(item => item.Id == registrationId && item.OtpAttempts == current.OtpAttempts &&
                               item.OtpHash == current.OtpHash && item.OtpExpiresAt > now)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(item => item.OtpAttempts, nextAttempt)
                    .SetProperty(item => item.OtpHash, nextAttempt >= MaxOtpAttempts ? string.Empty : current.OtpHash),
                    cancellationToken);

            if (affected == 1)
                return Result.Failure(nextAttempt >= MaxOtpAttempts
                    ? "Too many incorrect attempts. Request a new code."
                    : "The code is incorrect. Check the email and try again.");
        }

        return Result.Failure("Verification is busy. Please try again.");
    }

    private string SetNewOtp(PendingRegistration pending)
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        pending.OtpHash = Convert.ToHexString(ComputeOtpHash(pending.Id.ToString(), code));
        pending.OtpExpiresAt = DateTimeOffset.UtcNow.Add(OtpLifetime);
        pending.OtpAttempts = 0;
        return code;
    }

    private async Task<bool> SendOtpEmailAsync(string email, string fullName, string code, string subjectId, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            var safeCode = HtmlEncoder.Default.Encode(code);
            var safeName = HtmlEncoder.Default.Encode(fullName);
            var body = $"<p>Hello {safeName},</p><p>Use this six-digit code to verify your DevFolio account:</p><p style=\"font-size:28px;font-weight:bold;letter-spacing:8px\">{safeCode}</p><p>This code expires in five minutes. If you did not create this account, ignore this message.</p>";
            await _emailSender.SendAsync(email, "Your DevFolio email verification code", body, cancellationToken);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Could not send email verification code for pending registration {PendingRegistrationId}.", subjectId);
            return false;
        }
    }

    private bool VerifyOtp(string subjectId, string code, string expectedHash)
    {
        byte[] expected;
        try { expected = Convert.FromHexString(expectedHash); }
        catch (FormatException) { return false; }
        var submitted = ComputeOtpHash(subjectId, code);
        return expected.Length == submitted.Length && CryptographicOperations.FixedTimeEquals(expected, submitted);
    }

    private static bool TryReadOtp(string[]? parts, out long expiresAt, out int attempts, out string expectedHash)
    {
        expectedHash = string.Empty;
        expiresAt = 0;
        attempts = 0;
        if (parts is not { Length: 3 } || !long.TryParse(parts[0], out expiresAt) || !int.TryParse(parts[1], out attempts)) return false;
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expiresAt || attempts >= MaxOtpAttempts) return false;
        expectedHash = parts[2];
        try { return Convert.FromHexString(expectedHash).Length > 0; }
        catch (FormatException) { return false; }
    }

    private byte[] ComputeOtpHash(string subjectId, string code)
    {
        var signingKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");
        return HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingKey), Encoding.UTF8.GetBytes($"{subjectId}:{code}"));
    }
}
