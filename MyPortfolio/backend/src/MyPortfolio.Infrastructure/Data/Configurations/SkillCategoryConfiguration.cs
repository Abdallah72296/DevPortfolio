using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data.Configurations;

public class SkillCategoryConfiguration : BaseEntityConfiguration<SkillCategory>
{
    public override void Configure(EntityTypeBuilder<SkillCategory> builder)
    {
        base.Configure(builder);

        builder.ToTable("SkillCategories");

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.DisplayOrder)
            .IsRequired();

        builder.Property(c => c.IsVisible)
            .IsRequired();

        // 1-to-Many relationship with Skill
        builder.HasMany(c => c.Skills)
            .WithOne(s => s.SkillCategory)
            .HasForeignKey(s => s.SkillCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure EF Core to access the private backing field for encapsulated Skills collection
        builder.Navigation(c => c.Skills)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
