namespace MyPortfolio.Application.Portfolio.Models;

public record PublicExperienceDto(
    Guid Id,
    string JobTitle,
    string CompanyName,
    string Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder);
