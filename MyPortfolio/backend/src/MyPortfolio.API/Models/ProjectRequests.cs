using MyPortfolio.Application.Projects.Commands.ReorderProjects;

namespace MyPortfolio.API.Models;

public record CreateProjectRequest(
    string Title,
    string Description,
    string? Image,
    string? LiveUrl,
    string? GithubUrl,
    List<string>? Technologies,
    int DisplayOrder,
    bool IsFeatured);

public record UpdateProjectRequest(
    string Title,
    string Description,
    string? Image,
    string? LiveUrl,
    string? GithubUrl,
    List<string>? Technologies,
    int DisplayOrder,
    bool IsFeatured,
    bool IsVisible);

public record SetProjectVisibilityRequest(bool IsVisible);

public record SetProjectFeaturedRequest(bool IsFeatured);

public record ReorderProjectsRequest(List<ProjectOrderItem> Items);
