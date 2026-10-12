using MyPortfolio.Application.Educations.Commands.ReorderEducations;

namespace MyPortfolio.API.Models;

public record CreateEducationRequest(
    string InstitutionName,
    string Degree,
    string? FieldOfStudy,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder);

public record UpdateEducationRequest(
    string InstitutionName,
    string Degree,
    string? FieldOfStudy,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder);

public record SetEducationVisibilityRequest(bool IsVisible);

public record ReorderEducationsRequest(List<EducationOrderItem> Items);
