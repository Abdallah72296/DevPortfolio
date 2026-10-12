namespace MyPortfolio.API.Models;

public record CreateContactMessageRequest(
    string Name,
    string Email,
    string Subject,
    string Message,
    string? Username = null);
