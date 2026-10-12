using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.SocialLinks.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.SocialLinks.Commands.CreateSocialLink;

public record CreateSocialLinkCommand(
    string Platform,
    string Url,
    string? Icon,
    int DisplayOrder) : IRequest<Result<SocialLinkDto>>;

public class CreateSocialLinkCommandHandler : IRequestHandler<CreateSocialLinkCommand, Result<SocialLinkDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateSocialLinkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SocialLinkDto>> Handle(
        CreateSocialLinkCommand request,
        CancellationToken cancellationToken)
    {
        var socialLink = new SocialLink(
            platform: request.Platform,
            url: request.Url,
            icon: request.Icon,
            displayOrder: request.DisplayOrder,
            isVisible: true);

        _context.SocialLinks.Add(socialLink);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<SocialLinkDto>.Success(
            SocialLinkDto.FromEntity(socialLink),
            "Social link created successfully.");
    }
}
