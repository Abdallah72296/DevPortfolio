using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Educations.Models;

public record EducationDto(
    Guid Id,
    string InstitutionName,
    string Degree,
    string? FieldOfStudy,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder,
    bool IsVisible)
{
    public static EducationDto FromEntity(Education education) =>
        new(
            education.Id,
            education.InstitutionName,
            education.Degree,
            education.FieldOfStudy,
            education.Description,
            education.StartDate,
            education.EndDate,
            education.IsCurrent,
            education.DisplayOrder,
            education.IsVisible);
}
