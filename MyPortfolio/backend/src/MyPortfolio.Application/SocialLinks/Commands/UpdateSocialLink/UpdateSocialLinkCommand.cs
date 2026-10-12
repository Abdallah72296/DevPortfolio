using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.SocialLinks.Models;

namespace MyPortfolio.Application.SocialLinks.Commands.UpdateSocialLink;

public record UpdateSocialLinkCommand(
    Guid Id,
    string Platform,
    string Url,
    string? Icon,
    int DisplayOrder) : IRequest<Result<SocialLinkDto>>;

public class UpdateSocialLinkCommandHandler : IRequestHandler<UpdateSocialLinkCommand, Result<SocialLinkDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateSocialLinkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SocialLinkDto>> Handle(
        UpdateSocialLinkCommand request,
        CancellationToken cancellationToken)
    {
        var socialLink = await _context.SocialLinks
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (socialLink == null)
        {
            return Result<SocialLinkDto>.Failure($"Social link with ID '{request.Id}' was not found.");
        }

        socialLink.UpdateDetails(
            platform: request.Platform,
            url: request.Url,
            icon: request.Icon);

        socialLink.UpdateDisplayOrder(request.DisplayOrder);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<SocialLinkDto>.Success(
            SocialLinkDto.FromEntity(socialLink),
            "Social link updated successfully.");
    }
}
