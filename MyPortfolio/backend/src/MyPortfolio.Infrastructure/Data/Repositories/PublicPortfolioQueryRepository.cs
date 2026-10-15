using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Portfolio.Models;
using MyPortfolio.Application.Portfolio.Queries.GetPublicPortfolio;

namespace MyPortfolio.Infrastructure.Data.Repositories;

public sealed class PublicPortfolioQueryRepository : IPublicPortfolioQueryRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public PublicPortfolioQueryRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<PortfolioDto?> GetByOwnerUserIdAsync(
        string ownerUserId,
        bool requirePublished,
        CancellationToken cancellationToken)
    {
        var profileTask = QueryAsync(context => context.UserProfiles.IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == ownerUserId, cancellationToken), cancellationToken);
        var settingsTask = QueryAsync(context => context.PortfolioSiteSettings.IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(settings => settings.OwnerUserId == ownerUserId, cancellationToken), cancellationToken);

        await Task.WhenAll(profileTask, settingsTask);
        var profile = await profileTask;
        var settings = await settingsTask;
        if (profile is null || (requirePublished && settings?.IsPublished != true))
            return null;

        // These independent reads run on separate short-lived contexts. This avoids
        // serial network round-trips to the hosted SQL database without concurrent
        // operations on one EF Core DbContext.
        var sectionsTask = QueryAsync(context => context.PortfolioSections
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(section => section.OwnerUserId == ownerUserId && section.IsVisible)
            .OrderBy(section => section.DisplayOrder)
            .Select(section => new PublicPortfolioSectionDto(section.Key, section.Title, section.DisplayOrder))
            .ToListAsync(cancellationToken), cancellationToken);

        var skillCategoriesTask = QueryAsync(context => context.SkillCategories
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(category => category.OwnerUserId == ownerUserId && category.IsVisible)
            .OrderBy(category => category.DisplayOrder)
            .Select(category => new PublicSkillCategoryDto(
                category.Id,
                category.Name,
                category.DisplayOrder,
                category.Skills
                    .Where(skill => skill.OwnerUserId == ownerUserId && skill.IsVisible)
                    .OrderBy(skill => skill.DisplayOrder)
                    .Select(skill => new PublicSkillDto(skill.Id, skill.Name, skill.Icon, skill.DisplayOrder))
                    .ToList()))
            .ToListAsync(cancellationToken), cancellationToken);

        var experiencesTask = QueryAsync(context => context.Experiences
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(experience => experience.OwnerUserId == ownerUserId && experience.IsVisible)
            .OrderBy(experience => experience.DisplayOrder)
            .Select(experience => new PublicExperienceDto(
                experience.Id,
                experience.JobTitle,
                experience.CompanyName,
                experience.Description,
                experience.StartDate,
                experience.EndDate,
                experience.IsCurrent,
                experience.DisplayOrder))
            .ToListAsync(cancellationToken), cancellationToken);

        var projectsTask = QueryAsync(context => context.Projects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(project => project.OwnerUserId == ownerUserId && project.IsVisible)
            .OrderBy(project => project.DisplayOrder)
            .ToListAsync(cancellationToken), cancellationToken);

        var educationTask = QueryAsync(context => context.Educations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(item => item.OwnerUserId == ownerUserId && item.IsVisible)
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new PublicEducationDto(
                item.Id,
                item.InstitutionName,
                item.Degree,
                item.FieldOfStudy,
                item.Description,
                item.StartDate,
                item.EndDate,
                item.IsCurrent,
                item.DisplayOrder))
            .ToListAsync(cancellationToken), cancellationToken);

        var certificationsTask = QueryAsync(context => context.Certifications
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(item => item.OwnerUserId == ownerUserId && item.IsVisible)
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new PublicCertificationDto(
                item.Id,
                item.Name,
                item.IssuingOrganization,
                item.IssueDate,
                item.CredentialId,
                item.CredentialUrl,
                item.Description,
                item.DisplayOrder))
            .ToListAsync(cancellationToken), cancellationToken);

        var socialLinksTask = QueryAsync(context => context.SocialLinks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(item => item.OwnerUserId == ownerUserId && item.IsVisible)
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new PublicSocialLinkDto(item.Id, item.Platform, item.Url, item.Icon, item.DisplayOrder))
            .ToListAsync(cancellationToken), cancellationToken);

        await Task.WhenAll(
            sectionsTask,
            skillCategoriesTask,
            experiencesTask,
            projectsTask,
            educationTask,
            certificationsTask,
            socialLinksTask);

        var projectDtos = (await projectsTask).Select(project => new PublicProjectDto(
            project.Id,
            project.Title,
            project.Description,
            project.Image,
            project.LiveUrl,
            project.GithubUrl,
            project.Technologies.ToList(),
            project.DisplayOrder,
            project.IsFeatured,
            project.Images.ToList())).ToList();

        var publicProfile = new PublicUserProfileDto(
            profile.FullName,
            profile.Title,
            profile.HeadlineRole,
            profile.Badge,
            profile.Location,
            profile.CardCaption,
            profile.Email,
            profile.Phone,
            profile.Summary,
            profile.LongBio,
            profile.AvatarUrl,
            profile.CvUrl,
            profile.YearsOfExperience,
            DeserializeParagraphs(profile.ParagraphsJson, profile.Summary),
            profile.AvailableForHire);

        return new PortfolioDto(
            await sectionsTask,
            await skillCategoriesTask,
            await experiencesTask,
            projectDtos,
            await educationTask,
            await certificationsTask,
            await socialLinksTask,
            publicProfile,
            settings?.ThemeKey ?? "obsidian-dark",
            settings?.SeoTitle,
            settings?.SeoDescription,
            settings?.SeoKeywords);
    }

    private async Task<TResult> QueryAsync<TResult>(
        Func<ApplicationDbContext, Task<TResult>> query,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await query(context);
    }

    private static List<string> DeserializeParagraphs(string? paragraphsJson, string summary)
    {
        if (string.IsNullOrWhiteSpace(paragraphsJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(paragraphsJson) ?? [];
        }
        catch (JsonException)
        {
            return [summary];
        }
    }
}
