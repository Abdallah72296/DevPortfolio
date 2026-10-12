using MyPortfolio.Application.SocialLinks.Commands.ReorderSocialLinks;

namespace MyPortfolio.API.Models;

public record CreateSocialLinkRequest(
    string Platform,
    string Url,
    string? Icon,
    int DisplayOrder);

public record UpdateSocialLinkRequest(
    string Platform,
    string Url,
    string? Icon,
    int DisplayOrder);

public record SetSocialLinkVisibilityRequest(bool IsVisible);

public record ReorderSocialLinksRequest(List<SocialLinkOrderItem> Items);
