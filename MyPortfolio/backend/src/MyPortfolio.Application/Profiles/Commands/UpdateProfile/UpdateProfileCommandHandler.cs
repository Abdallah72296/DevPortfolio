using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Profiles.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Profiles.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<ProfileDto>>
{
    private readonly IGenericRepository<UserProfile> _profileRepository;
    private readonly IGenericRepository<PortfolioSiteSettings> _siteSettingsRepository;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        IGenericRepository<UserProfile> profileRepository,
        IGenericRepository<PortfolioSiteSettings> siteSettingsRepository,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _profileRepository = profileRepository;
        _siteSettingsRepository = siteSettingsRepository;
        _logger = logger;
    }

    public async Task<Result<ProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<ProfileDto>.Failure("User identifier is required to update profile.");
        }

        var profile = await _profileRepository.FirstOrDefaultAsync(
            item => item.UserId == userId,
            cancellationToken);

        var paragraphsJson = request.Paragraphs != null && request.Paragraphs.Count > 0
            ? JsonSerializer.Serialize(request.Paragraphs)
            : "[]";

        if (profile == null)
        {
            profile = new UserProfile(
                userId: userId,
                fullName: request.FullName,
                title: request.Title,
                headlineRole: request.HeadlineRole ?? request.Title,
                badge: request.Badge ?? "",
                location: request.Location ?? "",
                email: request.Email ?? "",
                phone: request.Phone ?? "",
                summary: request.Summary ?? "",
                longBio: request.LongBio ?? "",
                avatarUrl: request.AvatarUrl ?? "assets/images/developer_avatar.jpg",
                cvUrl: request.CvUrl ?? "#contact",
                yearsOfExperience: request.YearsOfExperience ?? "3+",
                paragraphsJson: paragraphsJson,
                availableForHire: request.AvailableForHire,
                cardCaption: request.CardCaption ?? $"{request.Title} • {request.Location}"
            );
            profile.AssignOwner(userId);

            await _profileRepository.AddAsync(profile, cancellationToken);
        }
        else
        {
            profile.UpdateProfile(
                fullName: request.FullName,
                title: request.Title,
                headlineRole: request.HeadlineRole ?? request.Title,
                badge: request.Badge ?? profile.Badge,
                location: request.Location ?? profile.Location,
                email: request.Email ?? profile.Email,
                phone: request.Phone ?? profile.Phone,
                summary: request.Summary ?? profile.Summary,
                longBio: request.LongBio ?? profile.LongBio,
                avatarUrl: request.AvatarUrl ?? profile.AvatarUrl,
                cvUrl: request.CvUrl ?? profile.CvUrl,
                yearsOfExperience: request.YearsOfExperience ?? profile.YearsOfExperience,
                paragraphsJson: paragraphsJson,
                availableForHire: request.AvailableForHire,
                cardCaption: request.CardCaption ?? profile.CardCaption
            );
            _profileRepository.Update(profile);
        }

        var settings = await _siteSettingsRepository.FirstOrDefaultAsync(
            item => item.OwnerUserId == userId,
            cancellationToken);
        if (settings == null)
        {
            settings = new PortfolioSiteSettings();
            settings.AssignOwner(userId);
            await _siteSettingsRepository.AddAsync(settings, cancellationToken);
        }
        settings.MarkPublished();
        _siteSettingsRepository.Update(settings);

        await _profileRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Profile for user {UserId} saved successfully via Generic Repository.", userId);

        return Result<ProfileDto>.Success(new ProfileDto
        {
            Id = profile.Id,
            FullName = profile.FullName,
            Title = profile.Title,
            HeadlineRole = profile.HeadlineRole,
            Badge = profile.Badge,
            Location = profile.Location,
            CardCaption = profile.CardCaption,
            Email = profile.Email,
            Phone = profile.Phone,
            YearsOfExperience = profile.YearsOfExperience,
            Summary = profile.Summary,
            LongBio = profile.LongBio,
            Paragraphs = request.Paragraphs ?? new List<string>(),
            AvatarUrl = profile.AvatarUrl,
            CvUrl = profile.CvUrl,
            AvailableForHire = profile.AvailableForHire
        }, "Profile updated successfully.");
    }
}
