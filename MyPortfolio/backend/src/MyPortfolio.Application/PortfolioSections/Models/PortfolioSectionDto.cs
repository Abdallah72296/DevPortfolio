using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.PortfolioSections.Models;

public class PortfolioSectionDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; }
    public bool IsSystemSection { get; set; }

    public static PortfolioSectionDto FromEntity(PortfolioSection section) => new()
    {
        Id = section.Id,
        Key = section.Key,
        Title = section.Title,
        DisplayOrder = section.DisplayOrder,
        IsVisible = section.IsVisible,
        IsSystemSection = section.IsSystemSection
    };
}
