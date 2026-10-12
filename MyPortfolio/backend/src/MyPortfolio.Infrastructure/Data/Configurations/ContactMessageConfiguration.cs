using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class ContactMessageConfiguration : BaseEntityConfiguration<ContactMessage>
{
    public override void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        base.Configure(builder);

        builder.ToTable("ContactMessages");

        builder.Property(m => m.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(m => m.Email)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(m => m.Subject)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(m => m.Message)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(m => m.IsRead)
            .IsRequired();

        builder.Property(m => m.ReadAt)
            .IsRequired(false);
    }
}
