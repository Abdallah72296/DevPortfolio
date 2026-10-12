using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Analytics.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Analytics.Queries.GetPortfolioAnalyticsSummary;

public record GetPortfolioAnalyticsSummaryQuery : IRequest<Result<PortfolioAnalyticsSummaryDto>>;

public class GetPortfolioAnalyticsSummaryQueryHandler
    : IRequestHandler<GetPortfolioAnalyticsSummaryQuery, Result<PortfolioAnalyticsSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPortfolioAnalyticsSummaryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<PortfolioAnalyticsSummaryDto>> Handle(
        GetPortfolioAnalyticsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var profileId = await _context.UserProfiles
            .AsNoTracking()
            .OrderBy(profile => profile.CreatedAt)
            .Select(profile => (Guid?)profile.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (profileId is null)
        {
            return Result<PortfolioAnalyticsSummaryDto>.Success(
                new PortfolioAnalyticsSummaryDto(0, 0, 0, 0, 0, periodStart));
        }

        var monthEvents = _context.PortfolioAnalyticsEvents
            .AsNoTracking()
            .Where(item => item.ProfileId == profileId && item.CreatedAt >= periodStart);

        var monthlyViews = await monthEvents.CountAsync(
            item => item.EventType == "portfolio_view", cancellationToken);
        var monthlyDownloads = await monthEvents.CountAsync(
            item => item.EventType == "cv_download", cancellationToken);
        var monthlyMessages = await _context.ContactMessages
            .AsNoTracking()
            .CountAsync(message => message.CreatedAt >= periodStart, cancellationToken);
        var totalMessages = await _context.ContactMessages
            .AsNoTracking()
            .CountAsync(cancellationToken);
        var unreadMessages = await _context.ContactMessages
            .AsNoTracking()
            .CountAsync(message => !message.IsRead, cancellationToken);

        return Result<PortfolioAnalyticsSummaryDto>.Success(
            new PortfolioAnalyticsSummaryDto(
                monthlyViews,
                monthlyDownloads,
                monthlyMessages,
                unreadMessages,
                totalMessages,
                periodStart));
    }
}
