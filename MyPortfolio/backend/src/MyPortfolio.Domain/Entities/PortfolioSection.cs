using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>
/// Represents a configurable section of the Portfolio website.
/// Allows the Admin Dashboard to dynamically reorder, rename, or toggle visibility of sections.
/// </summary>
public class PortfolioSection : BaseEntity
{
    public string Key { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }
    public bool IsSystemSection { get; private set; }

    // Parameterless constructor for EF Core / Deserialization
    private PortfolioSection()
    {
    }

    public PortfolioSection(string key, string title, int displayOrder, bool isVisible = true, bool isSystemSection = false)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Section key cannot be null or empty.", nameof(key));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Section title cannot be null or empty.", nameof(title));

        Key = key.Trim().ToLowerInvariant();
        Title = title.Trim();
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
        IsSystemSection = isSystemSection;
    }

    public PortfolioSection(Guid id, string key, string title, int displayOrder, bool isVisible, bool isSystemSection, DateTime createdAt)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Section key cannot be null or empty.", nameof(key));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Section title cannot be null or empty.", nameof(title));

        Key = key.Trim().ToLowerInvariant();
        Title = title.Trim();
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
        IsSystemSection = isSystemSection;
        CreatedAt = createdAt;
    }

    public void UpdateDetails(string title, int displayOrder, bool isVisible)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Section title cannot be null or empty.", nameof(title));

        Title = title.Trim();
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
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

    /// <summary>
    /// Checks whether this section can be permanently deleted.
    /// System/default sections are protected from permanent deletion.
    /// </summary>
    public bool CanDelete() => !IsSystemSection;
}
