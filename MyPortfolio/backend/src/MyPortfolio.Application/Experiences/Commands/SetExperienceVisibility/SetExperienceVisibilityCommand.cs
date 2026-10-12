using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Experiences.Models;

namespace MyPortfolio.Application.Experiences.Commands.SetExperienceVisibility;

public record SetExperienceVisibilityCommand(
    Guid Id,
    bool IsVisible) : IRequest<Result<ExperienceDto>>;

public class SetExperienceVisibilityCommandHandler : IRequestHandler<SetExperienceVisibilityCommand, Result<ExperienceDto>>
{
    private readonly IApplicationDbContext _context;

    public SetExperienceVisibilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExperienceDto>> Handle(
        SetExperienceVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var experience = await _context.Experiences
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (experience == null)
        {
            return Result<ExperienceDto>.Failure($"Experience with ID '{request.Id}' was not found.");
        }

        experience.SetVisibility(request.IsVisible);
        await _context.SaveChangesAsync(cancellationToken);

        var status = request.IsVisible ? "visible" : "hidden";
        return Result<ExperienceDto>.Success(
            ExperienceDto.FromEntity(experience),
            $"Experience '{experience.JobTitle}' visibility set to {status}.");
    }
}
