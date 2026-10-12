using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class ProjectConfiguration : BaseEntityConfiguration<Project>
{
    public override void Configure(EntityTypeBuilder<Project> builder)
    {
        base.Configure(builder);

        builder.ToTable("Projects");

        builder.Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(p => p.Image)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(p => p.ImagesJson)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Ignore(p => p.Images);

        builder.Property(p => p.LiveUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(p => p.GithubUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(p => p.DisplayOrder)
            .IsRequired();

        builder.Property(p => p.IsFeatured)
            .IsRequired();

        builder.Property(p => p.IsVisible)
            .IsRequired();

        // Map primitive string collection with private backing field
        builder.PrimitiveCollection(p => p.Technologies)
            .HasField("_technologies")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
