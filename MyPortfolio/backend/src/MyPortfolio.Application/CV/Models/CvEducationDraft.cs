namespace MyPortfolio.Application.CV.Models;

public record CvEducationDraft(
    string InstitutionName,
    string Degree,
    string? FieldOfStudy,
    string? Description,
    string? StartDate,
    string? EndDate,
    bool IsCurrent);
