using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class PortfolioSectionConfiguration : BaseEntityConfiguration<PortfolioSection>
{
    public override void Configure(EntityTypeBuilder<PortfolioSection> builder)
    {
        base.Configure(builder);

        builder.ToTable("PortfolioSections");

        builder.Property(s => s.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.Property(s => s.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.DisplayOrder)
            .IsRequired();

        builder.Property(s => s.IsVisible)
            .IsRequired();

        builder.Property(s => s.IsSystemSection)
            .IsRequired();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new PortfolioSection(
                Guid.Parse("10000000-0000-0000-0000-000000000001"),
                "hero",
                "Hero",
                1,
                isVisible: true,
                isSystemSection: true,
                seedDate),
            new PortfolioSection(
                Guid.Parse("10000000-0000-0000-0000-000000000002"),
                "about",
                "About",
                2,
                isVisible: true,
                isSystemSection: true,
                seedDate),
            new PortfolioSection(
                Guid.Parse("10000000-0000-0000-0000-000000000003"),
                "skills",
                "Skills",
                3,
                isVisible: true,
                isSystemSection: true,
                seedDate),
            new PortfolioSection(
                Guid.Parse("10000000-0000-0000-0000-000000000004"),
                "experience",
                "Experience",
                4,
                isVisible: true,
                isSystemSection: true,
                seedDate),
            new PortfolioSection(
                Guid.Parse("10000000-0000-0000-0000-000000000005"),
                "projects",
                "Projects",
                5,
                isVisible: true,
                isSystemSection: true,
                seedDate),
            new PortfolioSection(
                Guid.Parse("10000000-0000-0000-0000-000000000006"),
                "education",
                "Education",
                6,
                isVisible: true,
                isSystemSection: true,
                seedDate),
            new PortfolioSection(
                Guid.Parse("10000000-0000-0000-0000-000000000007"),
                "certifications",
                "Certifications",
                7,
                isVisible: true,
                isSystemSection: true,
                seedDate),
            new PortfolioSection(
                Guid.Parse("10000000-0000-0000-0000-000000000008"),
                "contact",
                "Contact",
                8,
                isVisible: true,
                isSystemSection: true,
                seedDate)
        );
    }
}
