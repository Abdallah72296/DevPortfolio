using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>
/// Represents an individual skill or technology belonging to a dynamic SkillCategory.
/// </summary>
public class Skill : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }
    public Guid SkillCategoryId { get; private set; }

    // Navigation property
    public SkillCategory? SkillCategory { get; private set; }

    // Parameterless constructor for EF Core
    private Skill()
    {
    }

    public Skill(string name, Guid skillCategoryId, int displayOrder = 0, string? icon = null, bool isVisible = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Skill name cannot be null or empty.", nameof(name));

        if (skillCategoryId == Guid.Empty)
            throw new ArgumentException("Skill category ID must be specified.", nameof(skillCategoryId));

        Name = name.Trim();
        SkillCategoryId = skillCategoryId;
        DisplayOrder = displayOrder;
        Icon = icon?.Trim();
        IsVisible = isVisible;
    }

    public void UpdateDetails(string name, string? icon, int displayOrder, bool isVisible)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Skill name cannot be null or empty.", nameof(name));

        Name = name.Trim();
        Icon = icon?.Trim();
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
        MarkUpdated();
    }

    public void MoveToCategory(Guid newCategoryId)
    {
        if (newCategoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty.", nameof(newCategoryId));

        if (SkillCategoryId == newCategoryId)
            return;

        // When moving via ID only, remove from existing in-memory category if attached
        if (SkillCategory != null)
        {
            SkillCategory.InternalRemoveSkill(this);
            SkillCategory = null;
        }

        SkillCategoryId = newCategoryId;
        MarkUpdated();
    }

    public void MoveToCategory(SkillCategory newCategory)
    {
        ArgumentNullException.ThrowIfNull(newCategory);

        if (newCategory.Id == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty.", nameof(newCategory));

        if (SkillCategoryId == newCategory.Id && SkillCategory == newCategory)
            return;

        // Remove from current category collection if attached
        if (SkillCategory != null && SkillCategory != newCategory)
        {
            SkillCategory.InternalRemoveSkill(this);
        }

        SkillCategoryId = newCategory.Id;
        SkillCategory = newCategory;

        // Add to the new category collection
        newCategory.InternalAddSkill(this);

        MarkUpdated();
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        MarkUpdated();
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;
        MarkUpdated();
    }
}
