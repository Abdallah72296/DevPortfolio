using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class PortfolioAnalyticsEventConfiguration : BaseEntityConfiguration<PortfolioAnalyticsEvent>
{
    public override void Configure(EntityTypeBuilder<PortfolioAnalyticsEvent> builder)
    {
        base.Configure(builder);
        builder.ToTable("PortfolioAnalyticsEvents");

        builder.Property(e => e.ProfileId).IsRequired();
        builder.Property(e => e.EventType).HasMaxLength(40).IsRequired();

        builder.HasIndex(e => new { e.ProfileId, e.EventType, e.CreatedAt });
        builder.HasOne<UserProfile>()
            .WithMany()
            .HasForeignKey(e => e.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
