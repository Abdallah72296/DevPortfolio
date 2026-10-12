using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Projects.Models;

namespace MyPortfolio.Application.Projects.Commands.SetProjectVisibility;

public record SetProjectVisibilityCommand(
    Guid Id,
    bool IsVisible) : IRequest<Result<ProjectDto>>;

public class SetProjectVisibilityCommandHandler : IRequestHandler<SetProjectVisibilityCommand, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;

    public SetProjectVisibilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProjectDto>> Handle(
        SetProjectVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
        {
            return Result<ProjectDto>.Failure($"Project with ID '{request.Id}' was not found.");
        }

        project.SetVisibility(request.IsVisible);
        await _context.SaveChangesAsync(cancellationToken);

        var status = request.IsVisible ? "visible" : "hidden";
        return Result<ProjectDto>.Success(
            ProjectDto.FromEntity(project),
            $"Project '{project.Title}' visibility set to {status}.");
    }
}
