using MediatR;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Profiles.Models;

namespace MyPortfolio.Application.Profiles.Commands.UpdateProfile;

public class UpdateProfileCommand : IRequest<Result<ProfileDto>>
{
    public string? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? HeadlineRole { get; set; }
    public string? Badge { get; set; }
    public string? Location { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Summary { get; set; }
    public string? LongBio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CvUrl { get; set; }
    public string? YearsOfExperience { get; set; }
    public List<string>? Paragraphs { get; set; }
    public bool AvailableForHire { get; set; } = true;
    public string? CardCaption { get; set; }
}
