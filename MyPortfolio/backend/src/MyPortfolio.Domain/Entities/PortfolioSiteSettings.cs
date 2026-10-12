using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>Appearance, SEO, domain and publication metadata for this single-tenant portfolio.</summary>
public sealed class PortfolioSiteSettings : BaseEntity
{
    public string ThemeKey { get; private set; } = "obsidian-dark";
    public string CustomDomain { get; private set; } = string.Empty;
    public string SeoTitle { get; private set; } = string.Empty;
    public string SeoDescription { get; private set; } = string.Empty;
    public string SeoKeywords { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    private PortfolioSiteSettings() { }

    public PortfolioSiteSettings(string themeKey = "obsidian-dark") => ThemeKey = themeKey;

    public void Update(string themeKey, string customDomain, string seoTitle, string seoDescription, string seoKeywords)
    {
        ThemeKey = themeKey.Trim();
        CustomDomain = customDomain.Trim().ToLowerInvariant();
        SeoTitle = seoTitle.Trim();
        SeoDescription = seoDescription.Trim();
        SeoKeywords = seoKeywords.Trim();
        MarkUpdated();
    }

    public void MarkPublished()
    {
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
        MarkUpdated();
    }
}
