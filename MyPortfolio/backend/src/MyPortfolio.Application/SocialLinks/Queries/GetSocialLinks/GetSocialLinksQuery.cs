using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.SocialLinks.Models;

namespace MyPortfolio.Application.SocialLinks.Queries.GetSocialLinks;

public record GetSocialLinksQuery : IRequest<Result<List<SocialLinkDto>>>;

public class GetSocialLinksQueryHandler : IRequestHandler<GetSocialLinksQuery, Result<List<SocialLinkDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSocialLinksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SocialLinkDto>>> Handle(
        GetSocialLinksQuery request,
        CancellationToken cancellationToken)
    {
        var links = await _context.SocialLinks
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken);

        var dtos = links.Select(SocialLinkDto.FromEntity).ToList();

        return Result<List<SocialLinkDto>>.Success(dtos);
    }
}
