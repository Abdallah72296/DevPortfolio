using MyPortfolio.Application.Experiences.Commands.ReorderExperiences;

namespace MyPortfolio.API.Models;

public record CreateExperienceRequest(
    string JobTitle,
    string CompanyName,
    string Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder);

public record UpdateExperienceRequest(
    string JobTitle,
    string CompanyName,
    string Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder);

public record SetExperienceVisibilityRequest(bool IsVisible);

public record ReorderExperiencesRequest(List<ExperienceOrderItem> Items);
