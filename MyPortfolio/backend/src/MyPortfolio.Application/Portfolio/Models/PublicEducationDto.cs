namespace MyPortfolio.Application.Portfolio.Models;

public record PublicEducationDto(
    Guid Id,
    string InstitutionName,
    string Degree,
    string? FieldOfStudy,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder);
