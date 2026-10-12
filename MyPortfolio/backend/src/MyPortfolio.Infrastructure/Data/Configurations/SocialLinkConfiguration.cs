using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class SocialLinkConfiguration : BaseEntityConfiguration<SocialLink>
{
    public override void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        base.Configure(builder);

        builder.ToTable("SocialLinks");

        builder.Property(s => s.Platform)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Url)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.Icon)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(s => s.DisplayOrder)
            .IsRequired();

        builder.Property(s => s.IsVisible)
            .IsRequired();
    }
}
