using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Experiences.Models;

public class ExperienceDto
{
    public Guid Id { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; }

    public static ExperienceDto FromEntity(Experience experience) => new()
    {
        Id = experience.Id,
        JobTitle = experience.JobTitle,
        CompanyName = experience.CompanyName,
        Description = experience.Description,
        StartDate = experience.StartDate,
        EndDate = experience.EndDate,
        IsCurrent = experience.IsCurrent,
        DisplayOrder = experience.DisplayOrder,
        IsVisible = experience.IsVisible
    };
}
