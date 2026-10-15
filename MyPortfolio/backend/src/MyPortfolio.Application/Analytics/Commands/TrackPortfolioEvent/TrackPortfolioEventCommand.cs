using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Analytics.Commands.TrackPortfolioEvent;

public static class PortfolioEventTypes
{
    public const string PortfolioView = "portfolio_view";
    public const string CvDownload = "cv_download";
}

public record TrackPortfolioEventCommand(string EventType, string? UserId = null) : IRequest<Result>;

public class TrackPortfolioEventCommandHandler : IRequestHandler<TrackPortfolioEventCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public TrackPortfolioEventCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(TrackPortfolioEventCommand request, CancellationToken cancellationToken)
    {
        if (request.EventType is not (PortfolioEventTypes.PortfolioView or PortfolioEventTypes.CvDownload))
            return Result.Failure("Unsupported analytics event.");

        var profiles = _context.UserProfiles.IgnoreQueryFilters().AsNoTracking();
        UserProfile? profile = null;
        if (!string.IsNullOrWhiteSpace(request.UserId))
            profile = await profiles.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
            return Result.Failure("Portfolio profile was not found.");

        var analyticsEvent = new PortfolioAnalyticsEvent(profile.Id, request.EventType);
        analyticsEvent.AssignOwner(profile.OwnerUserId);
        _context.PortfolioAnalyticsEvents.Add(analyticsEvent);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
