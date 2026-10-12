namespace MyPortfolio.Application.Portfolio.Models;

public record PublicSocialLinkDto(
    Guid Id,
    string Platform,
    string Url,
    string? Icon,
    int DisplayOrder);
