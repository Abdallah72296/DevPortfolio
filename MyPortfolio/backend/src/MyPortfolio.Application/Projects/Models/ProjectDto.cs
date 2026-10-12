using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Projects.Models;

public record ProjectDto(
    Guid Id,
    string Title,
    string Description,
    string? Image,
    string? LiveUrl,
    string? GithubUrl,
    List<string> Technologies,
    int DisplayOrder,
    bool IsFeatured,
    bool IsVisible,
    List<string>? Images = null)
{
    public static ProjectDto FromEntity(Project project) =>
        new(
            project.Id,
            project.Title,
            project.Description,
            project.Image,
            project.LiveUrl,
            project.GithubUrl,
            project.Technologies.ToList(),
            project.DisplayOrder,
            project.IsFeatured,
            project.IsVisible,
            project.Images.ToList());
}
