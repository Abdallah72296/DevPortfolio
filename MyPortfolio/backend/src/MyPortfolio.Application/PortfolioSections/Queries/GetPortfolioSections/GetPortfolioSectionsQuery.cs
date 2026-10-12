using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.PortfolioSections.Models;

namespace MyPortfolio.Application.PortfolioSections.Queries.GetPortfolioSections;

public record GetPortfolioSectionsQuery : IRequest<Result<List<PortfolioSectionDto>>>;

public class GetPortfolioSectionsQueryHandler : IRequestHandler<GetPortfolioSectionsQuery, Result<List<PortfolioSectionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetPortfolioSectionsQueryHandler(IApplicationDbContext _context)
    {
        this._context = _context;
    }

    public async Task<Result<List<PortfolioSectionDto>>> Handle(
        GetPortfolioSectionsQuery request,
        CancellationToken cancellationToken)
    {
        var sections = await _context.PortfolioSections
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new PortfolioSectionDto
            {
                Id = s.Id,
                Key = s.Key,
                Title = s.Title,
                DisplayOrder = s.DisplayOrder,
                IsVisible = s.IsVisible,
                IsSystemSection = s.IsSystemSection
            })
            .ToListAsync(cancellationToken);

        return Result<List<PortfolioSectionDto>>.Success(sections);
    }
}
