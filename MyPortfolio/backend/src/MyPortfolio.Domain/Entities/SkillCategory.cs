using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>
/// Represents a dynamic skill category (e.g., Backend, Frontend, Cloud & DevOps).
/// Not hardcoded to fixed types, allowing full admin control from the Dashboard.
/// </summary>
public class SkillCategory : BaseEntity
{
    private readonly List<Skill> _skills = [];

    public string Name { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }

    // Encapsulated collection: exposed as read-only to protect invariants
    public IReadOnlyCollection<Skill> Skills => _skills.AsReadOnly();

    // Parameterless constructor for EF Core
    private SkillCategory()
    {
    }

    public SkillCategory(string name, int displayOrder = 0, bool isVisible = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Skill category name cannot be null or empty.", nameof(name));

        Name = name.Trim();
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }

    public void UpdateDetails(string name, int displayOrder, bool isVisible)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Skill category name cannot be null or empty.", nameof(name));

        Name = name.Trim();
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
        MarkUpdated();
    }

    public Skill AddSkill(string name, string? icon = null, int displayOrder = 0, bool isVisible = true)
    {
        var skill = new Skill(name, Id, displayOrder, icon, isVisible);
        InternalAddSkill(skill);
        return skill;
    }

    public void AddSkill(Skill skill)
    {
        ArgumentNullException.ThrowIfNull(skill);
        skill.MoveToCategory(this);
    }

    public void RemoveSkill(Skill skill)
    {
        ArgumentNullException.ThrowIfNull(skill);
        InternalRemoveSkill(skill);
    }

    public void RemoveSkill(Guid skillId)
    {
        var skill = _skills.FirstOrDefault(s => s.Id == skillId);
        if (skill != null)
        {
            InternalRemoveSkill(skill);
        }
    }

    internal void InternalAddSkill(Skill skill)
    {
        if (!_skills.Any(s => s.Id == skill.Id))
        {
            _skills.Add(skill);
            MarkUpdated();
        }
    }

    internal void InternalRemoveSkill(Skill skill)
    {
        if (_skills.Remove(skill))
        {
            MarkUpdated();
        }
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
