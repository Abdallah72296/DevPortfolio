using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Experiences.Models;

namespace MyPortfolio.Application.Experiences.Commands.UpdateExperience;

public record UpdateExperienceCommand(
    Guid Id,
    string JobTitle,
    string CompanyName,
    string Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder) : IRequest<Result<ExperienceDto>>;

public class UpdateExperienceCommandHandler : IRequestHandler<UpdateExperienceCommand, Result<ExperienceDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateExperienceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExperienceDto>> Handle(
        UpdateExperienceCommand request,
        CancellationToken cancellationToken)
    {
        var experience = await _context.Experiences
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (experience == null)
        {
            return Result<ExperienceDto>.Failure($"Experience with ID '{request.Id}' was not found.");
        }

        // Use domain methods to preserve invariants
        experience.UpdateDetails(request.JobTitle, request.CompanyName, request.Description);
        experience.UpdatePeriod(request.StartDate, request.EndDate, request.IsCurrent);
        experience.UpdateDisplayOrder(request.DisplayOrder);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<ExperienceDto>.Success(
            ExperienceDto.FromEntity(experience),
            "Experience updated successfully.");
    }
}
