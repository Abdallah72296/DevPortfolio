namespace MyPortfolio.Application.Portfolio.Models;

public record PublicProjectDto(
    Guid Id,
    string Title,
    string Description,
    string? Image,
    string? LiveUrl,
    string? GithubUrl,
    List<string> Technologies,
    int DisplayOrder,
    bool IsFeatured,
    List<string>? Images = null);
