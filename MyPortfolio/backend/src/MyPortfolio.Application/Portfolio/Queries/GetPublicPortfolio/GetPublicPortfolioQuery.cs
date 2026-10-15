using MediatR;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Portfolio.Models;

namespace MyPortfolio.Application.Portfolio.Queries.GetPublicPortfolio;

public record GetPublicPortfolioQuery(string? UserId = null, bool RequirePublished = true) : IRequest<Result<PortfolioDto>>;

public class GetPublicPortfolioQueryHandler : IRequestHandler<GetPublicPortfolioQuery, Result<PortfolioDto>>
{
    private readonly IPublicPortfolioQueryRepository _portfolioRepository;

    public GetPublicPortfolioQueryHandler(IPublicPortfolioQueryRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<Result<PortfolioDto>> Handle(
        GetPublicPortfolioQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Result<PortfolioDto>.Failure("A portfolio owner is required.");

        var portfolio = await _portfolioRepository.GetByOwnerUserIdAsync(
            request.UserId.Trim(),
            request.RequirePublished,
            cancellationToken);

        return portfolio is null
            ? Result<PortfolioDto>.Failure("Portfolio was not found or is not publicly available.")
            : Result<PortfolioDto>.Success(portfolio);
    }
}
