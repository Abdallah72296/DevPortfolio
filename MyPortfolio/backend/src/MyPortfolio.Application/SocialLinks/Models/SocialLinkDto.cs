using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.SocialLinks.Models;

public record SocialLinkDto(
    Guid Id,
    string Platform,
    string Url,
    string? Icon,
    int DisplayOrder,
    bool IsVisible)
{
    public static SocialLinkDto FromEntity(SocialLink link) =>
        new(
            link.Id,
            link.Platform,
            link.Url,
            link.Icon,
            link.DisplayOrder,
            link.IsVisible);
}
