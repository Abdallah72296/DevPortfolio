namespace MyPortfolio.Application.CV.Models;

public record CvProjectDraft(
    string Title,
    string? Description,
    string? Image,
    string? LiveUrl,
    string? GithubUrl,
    List<string> Technologies,
    bool IsFeatured);
