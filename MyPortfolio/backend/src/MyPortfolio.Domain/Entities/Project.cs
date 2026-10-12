using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>
/// Represents a portfolio showcase project with links, technologies, and presentation settings.
/// </summary>
public class Project : BaseEntity
{
    private readonly List<string> _technologies = [];

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Image { get; private set; }
    public string? ImagesJson { get; private set; } = "[]";
    public string? LiveUrl { get; private set; }
    public string? GithubUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsFeatured { get; private set; }
    public bool IsVisible { get; private set; }

    public IReadOnlyList<string> Images
    {
        get
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(ImagesJson))
            {
                try
                {
                    list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(ImagesJson) ?? new();
                }
                catch { }
            }
            if (list.Count == 0 && !string.IsNullOrWhiteSpace(Image))
            {
                list.Add(Image);
            }
            return list.AsReadOnly();
        }
    }

    public void SetImages(IEnumerable<string>? images)
    {
        var cleanList = images?.Where(img => !string.IsNullOrWhiteSpace(img)).Select(img => img.Trim()).Distinct().ToList() ?? new();
        ImagesJson = System.Text.Json.JsonSerializer.Serialize(cleanList);
        Image = cleanList.FirstOrDefault();
        MarkUpdated();
    }

    // Encapsulated read-only collection of technology names
    public IReadOnlyCollection<string> Technologies => _technologies.AsReadOnly();

    // Parameterless constructor for EF Core / Deserialization
    private Project()
    {
    }

    public Project(
        string title,
        string description,
        string? image = null,
        string? liveUrl = null,
        string? githubUrl = null,
        int displayOrder = 0,
        bool isFeatured = false,
        bool isVisible = true)
    {
        ValidateRequiredFields(title, description);

        Title = title.Trim();
        Description = description.Trim();
        Image = image?.Trim();
        LiveUrl = liveUrl?.Trim();
        GithubUrl = githubUrl?.Trim();
        DisplayOrder = displayOrder;
        IsFeatured = isFeatured;
        IsVisible = isVisible;
    }

    public void UpdateDetails(
        string title,
        string description,
        string? image,
        int displayOrder,
        bool isFeatured,
        bool isVisible)
    {
        ValidateRequiredFields(title, description);

        Title = title.Trim();
        Description = description.Trim();
        Image = image?.Trim();
        DisplayOrder = displayOrder;
        IsFeatured = isFeatured;
        IsVisible = isVisible;
        MarkUpdated();
    }

    public void UpdateLinks(string? liveUrl, string? githubUrl)
    {
        LiveUrl = liveUrl?.Trim();
        GithubUrl = githubUrl?.Trim();
        MarkUpdated();
    }

    public void AddTechnology(string technology)
    {
        if (string.IsNullOrWhiteSpace(technology))
            throw new ArgumentException("Technology name cannot be null or empty.", nameof(technology));

        var trimmed = technology.Trim();
        if (!_technologies.Any(t => string.Equals(t, trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            _technologies.Add(trimmed);
            MarkUpdated();
        }
    }

    public void RemoveTechnology(string technology)
    {
        if (string.IsNullOrWhiteSpace(technology))
            return;

        var trimmed = technology.Trim();
        var existing = _technologies.FirstOrDefault(t => string.Equals(t, trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            _technologies.Remove(existing);
            MarkUpdated();
        }
    }

    public void SetFeatured(bool isFeatured)
    {
        IsFeatured = isFeatured;
        MarkUpdated();
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;
        MarkUpdated();
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        MarkUpdated();
    }

    private static void ValidateRequiredFields(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Project title cannot be null or empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Project description cannot be null or empty.", nameof(description));
    }
}
