using MyPortfolio.Application.Portfolio.Models;

namespace MyPortfolio.Application.Portfolio.Queries.GetPublicPortfolio;

public interface IPublicPortfolioQueryRepository
{
    Task<PortfolioDto?> GetByOwnerUserIdAsync(
        string ownerUserId,
        bool requirePublished,
        CancellationToken cancellationToken);
}
