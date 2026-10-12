using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>
/// Represents a public social or professional profile link.
/// Platform is dynamic (string) rather than enum to allow full admin configurability.
/// </summary>
public class SocialLink : BaseEntity
{
    public string Platform { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }

    // Parameterless constructor for EF Core / Deserialization
    private SocialLink()
    {
    }

    public SocialLink(
        string platform,
        string url,
        string? icon = null,
        int displayOrder = 0,
        bool isVisible = true)
    {
        ValidateRequiredFields(platform, url);

        Platform = platform.Trim();
        Url = url.Trim();
        Icon = icon?.Trim();
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }

    public void UpdateDetails(string platform, string url, string? icon)
    {
        ValidateRequiredFields(platform, url);

        Platform = platform.Trim();
        Url = url.Trim();
        Icon = icon?.Trim();
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

    private static void ValidateRequiredFields(string platform, string url)
    {
        if (string.IsNullOrWhiteSpace(platform))
            throw new ArgumentException("Platform cannot be null or empty.", nameof(platform));

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
    }
}
