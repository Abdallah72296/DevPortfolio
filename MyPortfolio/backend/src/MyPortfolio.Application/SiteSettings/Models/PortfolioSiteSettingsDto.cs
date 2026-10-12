using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.SiteSettings.Models;

public sealed record PortfolioSiteSettingsDto(
    string ThemeKey,
    string CustomDomain,
    string SeoTitle,
    string SeoDescription,
    string SeoKeywords,
    bool IsPublished,
    DateTime? PublishedAt)
{
    public static PortfolioSiteSettingsDto From(PortfolioSiteSettings settings) => new(
        settings.ThemeKey,
        settings.CustomDomain,
        settings.SeoTitle,
        settings.SeoDescription,
        settings.SeoKeywords,
        settings.IsPublished,
        settings.PublishedAt);
}
