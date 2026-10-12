using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class SkillConfiguration : BaseEntityConfiguration<Skill>
{
    public override void Configure(EntityTypeBuilder<Skill> builder)
    {
        base.Configure(builder);

        builder.ToTable("Skills");

        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Icon)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(s => s.DisplayOrder)
            .IsRequired();

        builder.Property(s => s.IsVisible)
            .IsRequired();

        builder.Property(s => s.SkillCategoryId)
            .IsRequired();

        builder.HasIndex(s => s.SkillCategoryId);
    }
}
