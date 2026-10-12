using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.SocialLinks.Commands.DeleteSocialLink;

public record DeleteSocialLinkCommand(Guid Id) : IRequest<Result>;

public class DeleteSocialLinkCommandHandler : IRequestHandler<DeleteSocialLinkCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteSocialLinkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteSocialLinkCommand request,
        CancellationToken cancellationToken)
    {
        var socialLink = await _context.SocialLinks
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (socialLink == null)
        {
            return Result.Failure($"Social link with ID '{request.Id}' was not found.");
        }

        _context.SocialLinks.Remove(socialLink);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success($"Social link '{socialLink.Platform}' deleted successfully.");
    }
}
