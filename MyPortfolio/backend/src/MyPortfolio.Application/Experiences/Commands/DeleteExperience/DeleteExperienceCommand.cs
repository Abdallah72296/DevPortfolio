using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Experiences.Commands.DeleteExperience;

public record DeleteExperienceCommand(Guid Id) : IRequest<Result>;

public class DeleteExperienceCommandHandler : IRequestHandler<DeleteExperienceCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteExperienceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteExperienceCommand request,
        CancellationToken cancellationToken)
    {
        var experience = await _context.Experiences
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (experience == null)
        {
            return Result.Failure($"Experience with ID '{request.Id}' was not found.");
        }

        _context.Experiences.Remove(experience);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success($"Experience '{experience.JobTitle}' deleted successfully.");
    }
}
