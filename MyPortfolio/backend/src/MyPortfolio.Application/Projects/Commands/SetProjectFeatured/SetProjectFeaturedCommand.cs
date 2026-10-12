using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Projects.Models;

namespace MyPortfolio.Application.Projects.Commands.SetProjectFeatured;

public record SetProjectFeaturedCommand(
    Guid Id,
    bool IsFeatured) : IRequest<Result<ProjectDto>>;

public class SetProjectFeaturedCommandHandler : IRequestHandler<SetProjectFeaturedCommand, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;

    public SetProjectFeaturedCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProjectDto>> Handle(
        SetProjectFeaturedCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
        {
            return Result<ProjectDto>.Failure($"Project with ID '{request.Id}' was not found.");
        }

        project.SetFeatured(request.IsFeatured);
        await _context.SaveChangesAsync(cancellationToken);

        var status = request.IsFeatured ? "featured" : "standard";
        return Result<ProjectDto>.Success(
            ProjectDto.FromEntity(project),
            $"Project '{project.Title}' featured status set to {status}.");
    }
}
