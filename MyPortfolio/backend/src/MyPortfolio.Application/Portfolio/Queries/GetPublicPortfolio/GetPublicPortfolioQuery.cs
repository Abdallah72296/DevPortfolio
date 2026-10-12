using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Portfolio.Models;

namespace MyPortfolio.Application.Portfolio.Queries.GetPublicPortfolio;

public record GetPublicPortfolioQuery(string? Username = null) : IRequest<Result<PortfolioDto>>;

public class GetPublicPortfolioQueryHandler : IRequestHandler<GetPublicPortfolioQuery, Result<PortfolioDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicPortfolioQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PortfolioDto>> Handle(
        GetPublicPortfolioQuery request,
        CancellationToken cancellationToken)
    {
        MyPortfolio.Domain.Entities.UserProfile? profileEntity = null;
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var target = request.Username.Trim().ToLowerInvariant();
            profileEntity = await _context.UserProfiles.IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId.ToLower() == target
                    || p.Email.ToLower() == target
                    || p.Email.ToLower().StartsWith(target + "@")
                    || p.FullName.ToLower() == target, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(request.Username))
            profileEntity ??= await _context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var ownerUserId = profileEntity?.OwnerUserId;

        var sections = await _context.PortfolioSections
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => ownerUserId != null && s.OwnerUserId == ownerUserId && s.IsVisible)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new PublicPortfolioSectionDto(
                s.Key,
                s.Title,
                s.DisplayOrder))
            .ToListAsync(cancellationToken);

        var skillCategories = await _context.SkillCategories
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => ownerUserId != null && c.OwnerUserId == ownerUserId && c.IsVisible)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new PublicSkillCategoryDto(
                c.Id,
                c.Name,
                c.DisplayOrder,
                c.Skills
                    .Where(s => s.OwnerUserId == ownerUserId && s.IsVisible)
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new PublicSkillDto(
                        s.Id,
                        s.Name,
                        s.Icon,
                        s.DisplayOrder))
                    .ToList()))
            .ToListAsync(cancellationToken);

        var experiences = await _context.Experiences
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(e => ownerUserId != null && e.OwnerUserId == ownerUserId && e.IsVisible)
            .OrderBy(e => e.DisplayOrder)
            .Select(e => new PublicExperienceDto(
                e.Id,
                e.JobTitle,
                e.CompanyName,
                e.Description,
                e.StartDate,
                e.EndDate,
                e.IsCurrent,
                e.DisplayOrder))
            .ToListAsync(cancellationToken);

        var projects = await _context.Projects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => ownerUserId != null && p.OwnerUserId == ownerUserId && p.IsVisible)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        var projectDtos = projects.Select(p => new PublicProjectDto(
            p.Id,
            p.Title,
            p.Description,
            p.Image,
            p.LiveUrl,
            p.GithubUrl,
            p.Technologies.ToList(),
            p.DisplayOrder,
            p.IsFeatured,
            p.Images.ToList())).ToList();

        var educations = await _context.Educations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(e => ownerUserId != null && e.OwnerUserId == ownerUserId && e.IsVisible)
            .OrderBy(e => e.DisplayOrder)
            .Select(e => new PublicEducationDto(
                e.Id,
                e.InstitutionName,
                e.Degree,
                e.FieldOfStudy,
                e.Description,
                e.StartDate,
                e.EndDate,
                e.IsCurrent,
                e.DisplayOrder))
            .ToListAsync(cancellationToken);

        var certifications = await _context.Certifications
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => ownerUserId != null && c.OwnerUserId == ownerUserId && c.IsVisible)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new PublicCertificationDto(
                c.Id,
                c.Name,
                c.IssuingOrganization,
                c.IssueDate,
                c.CredentialId,
                c.CredentialUrl,
                c.Description,
                c.DisplayOrder))
            .ToListAsync(cancellationToken);

        var socialLinks = await _context.SocialLinks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => ownerUserId != null && s.OwnerUserId == ownerUserId && s.IsVisible)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new PublicSocialLinkDto(
                s.Id,
                s.Platform,
                s.Url,
                s.Icon,
                s.DisplayOrder))
            .ToListAsync(cancellationToken);

        PublicUserProfileDto? publicProfile = null;
        if (profileEntity != null)
        {
            var paragraphs = new List<string>();
            if (!string.IsNullOrWhiteSpace(profileEntity.ParagraphsJson))
            {
                try
                {
                    paragraphs = System.Text.Json.JsonSerializer.Deserialize<List<string>>(profileEntity.ParagraphsJson) ?? new();
                }
                catch
                {
                    paragraphs = new List<string> { profileEntity.Summary };
                }
            }

            publicProfile = new PublicUserProfileDto(
                profileEntity.FullName,
                profileEntity.Title,
                profileEntity.HeadlineRole,
                profileEntity.Badge,
                profileEntity.Location,
                profileEntity.CardCaption,
                profileEntity.Email,
                profileEntity.Phone,
                profileEntity.Summary,
                profileEntity.LongBio,
                profileEntity.AvatarUrl,
                profileEntity.CvUrl,
                profileEntity.YearsOfExperience,
                paragraphs,
                profileEntity.AvailableForHire);
        }

        var portfolioDto = new PortfolioDto(
            sections,
            skillCategories,
            experiences,
            projectDtos,
            educations,
            certifications,
            socialLinks,
            publicProfile);

        return Result<PortfolioDto>.Success(portfolioDto);
    }
}
