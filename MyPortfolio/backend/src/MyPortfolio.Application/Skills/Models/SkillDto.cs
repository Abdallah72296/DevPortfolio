using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Skills.Models;

public class SkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; }
    public Guid SkillCategoryId { get; set; }

    public static SkillDto FromEntity(Skill skill) => new()
    {
        Id = skill.Id,
        Name = skill.Name,
        Icon = skill.Icon,
        DisplayOrder = skill.DisplayOrder,
        IsVisible = skill.IsVisible,
        SkillCategoryId = skill.SkillCategoryId
    };
}
