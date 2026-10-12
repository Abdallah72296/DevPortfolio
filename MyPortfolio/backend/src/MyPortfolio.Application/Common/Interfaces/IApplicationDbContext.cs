using Microsoft.EntityFrameworkCore;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Common.Interfaces;

/// <summary>
/// Abstraction for the application persistence layer, allowing Application handlers
/// to interact with database entities without coupling to Infrastructure implementation details.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<PortfolioSection> PortfolioSections { get; }
    DbSet<SkillCategory> SkillCategories { get; }
    DbSet<Skill> Skills { get; }
    DbSet<Experience> Experiences { get; }
    DbSet<Project> Projects { get; }
    DbSet<Education> Educations { get; }
    DbSet<Certification> Certifications { get; }
    DbSet<SocialLink> SocialLinks { get; }
    DbSet<ContactMessage> ContactMessages { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<PortfolioAnalyticsEvent> PortfolioAnalyticsEvents { get; }
    DbSet<PortfolioSiteSettings> PortfolioSiteSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
