using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

public class PortfolioAnalyticsEvent : BaseEntity
{
    public Guid ProfileId { get; private set; }
    public string EventType { get; private set; } = string.Empty;

    private PortfolioAnalyticsEvent()
    {
    }

    public PortfolioAnalyticsEvent(Guid profileId, string eventType)
    {
        if (profileId == Guid.Empty)
            throw new ArgumentException("A profile is required to record analytics.", nameof(profileId));
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("An event type is required.", nameof(eventType));

        ProfileId = profileId;
        EventType = eventType.Trim();
    }
}
