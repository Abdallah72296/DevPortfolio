using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class UserProfileConfiguration : BaseEntityConfiguration<UserProfile>
{
    public override void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserProfiles");

        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.HeadlineRole)
            .HasMaxLength(200);

        builder.Property(p => p.Badge)
            .HasMaxLength(300);

        builder.Property(p => p.Location)
            .HasMaxLength(200);

        builder.Property(p => p.CardCaption)
            .HasMaxLength(200);

        builder.Property(p => p.Email)
            .HasMaxLength(256);

        builder.Property(p => p.Phone)
            .HasMaxLength(50);

        builder.Property(p => p.Summary)
            .HasMaxLength(2000);

        builder.Property(p => p.LongBio)
            .HasMaxLength(4000);

        builder.Property(p => p.AvatarUrl)
            .HasMaxLength(1000);

        builder.Property(p => p.CvUrl)
            .HasMaxLength(1000);

        builder.Property(p => p.YearsOfExperience)
            .HasMaxLength(50);

        builder.Property(p => p.ParagraphsJson)
            .HasMaxLength(4000);

        builder.HasIndex(p => p.UserId);
    }
}
