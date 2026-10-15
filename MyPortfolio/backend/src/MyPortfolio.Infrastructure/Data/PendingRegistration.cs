namespace MyPortfolio.Infrastructure.Data;

/// <summary>
/// Temporary signup data. It is removed after email verification or expiry.
/// PasswordHash is produced by ASP.NET Identity; plaintext passwords are never stored.
/// </summary>
public sealed class PendingRegistration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string NormalizedUsername { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string OtpHash { get; set; } = string.Empty;
    public DateTimeOffset OtpExpiresAt { get; set; }
    public int OtpAttempts { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
