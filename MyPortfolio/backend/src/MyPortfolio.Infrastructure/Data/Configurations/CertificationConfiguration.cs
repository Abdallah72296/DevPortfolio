using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class CertificationConfiguration : BaseEntityConfiguration<Certification>
{
    public override void Configure(EntityTypeBuilder<Certification> builder)
    {
        base.Configure(builder);

        builder.ToTable("Certifications");

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.IssuingOrganization)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.IssueDate)
            .IsRequired();

        builder.Property(c => c.CredentialId)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(c => c.CredentialUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.Description)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(c => c.DisplayOrder)
            .IsRequired();

        builder.Property(c => c.IsVisible)
            .IsRequired();
    }
}
