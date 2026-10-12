namespace MyPortfolio.API.Models;

public record UpdatePortfolioSectionRequest(string Title, int DisplayOrder);

public record SetPortfolioSectionVisibilityRequest(bool IsVisible);
