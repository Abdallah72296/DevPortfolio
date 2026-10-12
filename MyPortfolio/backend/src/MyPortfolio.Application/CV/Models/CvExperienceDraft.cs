namespace MyPortfolio.Application.CV.Models;

public record CvExperienceDraft(
    string JobTitle,
    string CompanyName,
    string? Description,
    string? StartDate,
    string? EndDate,
    bool IsCurrent);
