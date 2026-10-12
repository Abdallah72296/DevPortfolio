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
        var query = _profileRepository.Query().AsNoTracking();

        UserProfile? profile = null;

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            profile = await query.FirstOrDefaultAsync(item => item.UserId == request.UserId, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.QueryUser))
        {
            profile = await query.IgnoreQueryFilters().FirstOrDefaultAsync(
                item => item.UserId == request.QueryUser || item.Email == request.QueryUser || item.FullName == request.QueryUser,
                cancellationToken);
        }
        else
        {
            profile = await query.FirstOrDefaultAsync(cancellationToken);
        }

        if (profile == null)
        {
            // Default placeholder profile
            return Result<ProfileDto>.Success(new ProfileDto
            {
                FullName = "Abdallah Abdeltawab",
                Title = ".NET Backend Developer",
                HeadlineRole = ".NET Backend Developer",
                Badge = ".NET BACKEND DEVELOPER • CLEAN ARCHITECTURE",
                Location = "Giza, Egypt (Remote & Worldwide)",
                CardCaption = ".NET Backend Developer • Giza, Egypt",
                Email = "abdallahabdeltawab6@gmail.com",
                Phone = "+20 1002215065",
                YearsOfExperience = "3+",
                Summary = ".NET Backend Developer with professional and freelance experience building backend systems using C#, ASP.NET Core Web API, EF Core, Clean Architecture, CQRS, and RabbitMQ.",
                LongBio = "I am a passionate .NET Backend Developer with professional and freelance experience building robust enterprise backend architectures.",
                Paragraphs = new List<string>
                {
                    ".NET Backend Developer with professional and freelance experience building robust enterprise backend systems using C#, ASP.NET Core Web API, EF Core, Clean Architecture, CQRS, and RabbitMQ.",
                    "Experienced in financial, banking, project management, and business systems, with hands-on experience in RESTful API development, external banking integrations, asynchronous workflows, authentication, and relational databases.",
                    "Proven track record delivering mission-critical applications at Jedra and international freelance clients, including RabbitMQ-based pipelines and financial transaction aggregation engines. Military Service: Completed."
                },
                AvatarUrl = "assets/images/developer_avatar.jpg",
                CvUrl = "#contact",
                AvailableForHire = true
            });
        }

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
