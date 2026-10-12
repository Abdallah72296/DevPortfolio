using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.SocialLinks.Models;

namespace MyPortfolio.Application.SocialLinks.Commands.SetSocialLinkVisibility;

public record SetSocialLinkVisibilityCommand(
    Guid Id,
    bool IsVisible) : IRequest<Result<SocialLinkDto>>;

public class SetSocialLinkVisibilityCommandHandler : IRequestHandler<SetSocialLinkVisibilityCommand, Result<SocialLinkDto>>
{
    private readonly IApplicationDbContext _context;

    public SetSocialLinkVisibilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SocialLinkDto>> Handle(
        SetSocialLinkVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var socialLink = await _context.SocialLinks
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (socialLink == null)
        {
            return Result<SocialLinkDto>.Failure($"Social link with ID '{request.Id}' was not found.");
        }

        socialLink.SetVisibility(request.IsVisible);
        await _context.SaveChangesAsync(cancellationToken);

        var status = request.IsVisible ? "visible" : "hidden";
        return Result<SocialLinkDto>.Success(
            SocialLinkDto.FromEntity(socialLink),
            $"Social link '{socialLink.Platform}' visibility set to {status}.");
    }
}
