using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class ExperienceConfiguration : BaseEntityConfiguration<Experience>
{
    public override void Configure(EntityTypeBuilder<Experience> builder)
    {
        base.Configure(builder);

        builder.ToTable("Experiences");

        builder.Property(e => e.JobTitle)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(e => e.CompanyName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.StartDate)
            .IsRequired();

        builder.Property(e => e.EndDate)
            .IsRequired(false);

        builder.Property(e => e.IsCurrent)
            .IsRequired();

        builder.Property(e => e.DisplayOrder)
            .IsRequired();

        builder.Property(e => e.IsVisible)
            .IsRequired();
    }
}
