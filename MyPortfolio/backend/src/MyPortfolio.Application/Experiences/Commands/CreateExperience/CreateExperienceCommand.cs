using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Experiences.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Experiences.Commands.CreateExperience;

public record CreateExperienceCommand(
    string JobTitle,
    string CompanyName,
    string Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder) : IRequest<Result<ExperienceDto>>;

public class CreateExperienceCommandHandler : IRequestHandler<CreateExperienceCommand, Result<ExperienceDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateExperienceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExperienceDto>> Handle(
        CreateExperienceCommand request,
        CancellationToken cancellationToken)
    {
        var experience = new Experience(
            jobTitle: request.JobTitle,
            companyName: request.CompanyName,
            description: request.Description,
            startDate: request.StartDate,
            endDate: request.EndDate,
            isCurrent: request.IsCurrent,
            displayOrder: request.DisplayOrder,
            isVisible: true);

        _context.Experiences.Add(experience);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ExperienceDto>.Success(
            ExperienceDto.FromEntity(experience),
            "Experience created successfully.");
    }
}
