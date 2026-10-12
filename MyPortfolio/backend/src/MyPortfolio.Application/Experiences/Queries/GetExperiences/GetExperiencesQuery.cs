using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Experiences.Models;

namespace MyPortfolio.Application.Experiences.Queries.GetExperiences;

public record GetExperiencesQuery : IRequest<Result<List<ExperienceDto>>>;

public class GetExperiencesQueryHandler : IRequestHandler<GetExperiencesQuery, Result<List<ExperienceDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetExperiencesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ExperienceDto>>> Handle(
        GetExperiencesQuery request,
        CancellationToken cancellationToken)
    {
        var experiences = await _context.Experiences
            .AsNoTracking()
            .OrderBy(e => e.DisplayOrder)
            .Select(e => new ExperienceDto
            {
                Id = e.Id,
                JobTitle = e.JobTitle,
                CompanyName = e.CompanyName,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsCurrent = e.IsCurrent,
                DisplayOrder = e.DisplayOrder,
                IsVisible = e.IsVisible
            })
            .ToListAsync(cancellationToken);

        return Result<List<ExperienceDto>>.Success(experiences);
    }
}
