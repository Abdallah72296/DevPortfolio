namespace MyPortfolio.Application.Authentication.Models;

public sealed record RegistrationResponseDto(string Email, bool ConfirmationEmailSent);
