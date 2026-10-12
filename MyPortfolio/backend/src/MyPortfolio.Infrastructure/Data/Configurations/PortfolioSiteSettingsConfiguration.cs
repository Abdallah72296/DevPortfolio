using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public sealed class PortfolioSiteSettingsConfiguration : BaseEntityConfiguration<PortfolioSiteSettings>
{
    public override void Configure(EntityTypeBuilder<PortfolioSiteSettings> builder)
    {
        base.Configure(builder);
        builder.ToTable("PortfolioSiteSettings");
        builder.Property(x => x.ThemeKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.CustomDomain).HasMaxLength(253).IsRequired();
        builder.Property(x => x.SeoTitle).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SeoDescription).HasMaxLength(500).IsRequired();
        builder.Property(x => x.SeoKeywords).HasMaxLength(1000).IsRequired();
        builder.HasIndex(x => x.CustomDomain).IsUnique().HasFilter("[CustomDomain] <> ''");
    }
}
