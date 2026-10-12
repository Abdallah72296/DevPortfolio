using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Projects.Models;

namespace MyPortfolio.Application.Projects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<Result<List<ProjectDto>>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, Result<List<ProjectDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProjectDto>>> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        var dtos = projects.Select(ProjectDto.FromEntity).ToList();

        return Result<List<ProjectDto>>.Success(dtos);
    }
}
