using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Profiles.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Profiles.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<ProfileDto>>
{
    private readonly IGenericRepository<UserProfile> _profileRepository;

    public GetProfileQueryHandler(IGenericRepository<UserProfile> profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<Result<ProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Result<ProfileDto>.Failure("User identifier is required.");

        var profile = await _profileRepository.Query().AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == request.UserId, cancellationToken);

        if (profile == null)
            return Result<ProfileDto>.Failure("Profile was not found for the authenticated user.");

        List<string> paragraphsList = new();
        if (!string.IsNullOrWhiteSpace(profile.ParagraphsJson))
        {
            try
            {
                paragraphsList = JsonSerializer.Deserialize<List<string>>(profile.ParagraphsJson) ?? new();
            }
            catch
            {
                paragraphsList = new List<string> { profile.Summary };
            }
        }

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
            Paragraphs = paragraphsList,
            AvatarUrl = profile.AvatarUrl,
            CvUrl = profile.CvUrl,
            AvailableForHire = profile.AvailableForHire
        });
    }
}
