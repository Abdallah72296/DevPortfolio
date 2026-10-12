namespace MyPortfolio.Application.Authentication.Models;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
}
