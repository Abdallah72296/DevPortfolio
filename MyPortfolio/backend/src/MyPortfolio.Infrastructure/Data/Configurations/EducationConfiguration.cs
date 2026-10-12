using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class EducationConfiguration : BaseEntityConfiguration<Education>
{
    public override void Configure(EntityTypeBuilder<Education> builder)
    {
        base.Configure(builder);

        builder.ToTable("Educations");

        builder.Property(e => e.InstitutionName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Degree)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(e => e.FieldOfStudy)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(e => e.Description)
            .HasMaxLength(2000)
            .IsRequired(false);

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
