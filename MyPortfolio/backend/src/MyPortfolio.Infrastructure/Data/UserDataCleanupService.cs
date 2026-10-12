using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Domain.Common;

namespace MyPortfolio.Infrastructure.Data;

public class UserDataCleanupService : IUserDataCleanupService
{
    private readonly ApplicationDbContext _context;

    public UserDataCleanupService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CleanupUserDataAsync(string userId, CancellationToken cancellationToken = default)
    {
        RemoveOwned(_context.PortfolioSections, userId);
        RemoveOwned(_context.SkillCategories, userId);
        RemoveOwned(_context.Skills, userId);
        RemoveOwned(_context.Experiences, userId);
        RemoveOwned(_context.Projects, userId);
        RemoveOwned(_context.Educations, userId);
        RemoveOwned(_context.Certifications, userId);
        RemoveOwned(_context.SocialLinks, userId);
        RemoveOwned(_context.ContactMessages, userId);
        RemoveOwned(_context.PortfolioAnalyticsEvents, userId);
        RemoveOwned(_context.PortfolioSiteSettings, userId);
        RemoveOwned(_context.UserProfiles, userId);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void RemoveOwned<TEntity>(DbSet<TEntity> entities, string userId) where TEntity : BaseEntity
    {
        entities.RemoveRange(entities.IgnoreQueryFilters().Where(entity => entity.OwnerUserId == userId));
    }
}
