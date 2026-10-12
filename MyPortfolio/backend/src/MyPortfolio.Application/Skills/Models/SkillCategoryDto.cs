using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Skills.Models;

public class SkillCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; }
    public List<SkillDto> Skills { get; set; } = [];

    public static SkillCategoryDto FromEntity(SkillCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        DisplayOrder = category.DisplayOrder,
        IsVisible = category.IsVisible,
        Skills = category.Skills
            .OrderBy(s => s.DisplayOrder)
            .Select(SkillDto.FromEntity)
            .ToList()
    };
}
