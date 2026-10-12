using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Domain.Entities;
using MyPortfolio.Domain.Common;

namespace MyPortfolio.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>, IApplicationDbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? CurrentUserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public DbSet<PortfolioSection> PortfolioSections => Set<PortfolioSection>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<PortfolioAnalyticsEvent> PortfolioAnalyticsEvents => Set<PortfolioAnalyticsEvent>();
    public DbSet<PortfolioSiteSettings> PortfolioSiteSettings => Set<PortfolioSiteSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        var filterMethod = GetType().GetMethod(nameof(ApplyOwnerFilter), BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var entityType in builder.Model.GetEntityTypes().Where(type => typeof(BaseEntity).IsAssignableFrom(type.ClrType)))
        {
            filterMethod.MakeGenericMethod(entityType.ClrType).Invoke(this, [builder]);
        }
    }

    private void ApplyOwnerFilter<TEntity>(ModelBuilder builder) where TEntity : BaseEntity =>
        builder.Entity<TEntity>().HasQueryFilter(entity => CurrentUserId == null || entity.OwnerUserId == CurrentUserId);

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ownerId = CurrentUserId;
        if (!string.IsNullOrWhiteSpace(ownerId))
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(entry => entry.State == EntityState.Added && string.IsNullOrWhiteSpace(entry.Entity.OwnerUserId)))
                entry.Entity.AssignOwner(ownerId);
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
