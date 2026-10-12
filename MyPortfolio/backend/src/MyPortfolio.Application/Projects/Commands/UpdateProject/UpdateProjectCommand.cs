using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Projects.Models;

namespace MyPortfolio.Application.Projects.Commands.UpdateProject;

public record UpdateProjectCommand(
    Guid Id,
    string Title,
    string Description,
    string? Image,
    string? LiveUrl,
    string? GithubUrl,
    List<string>? Technologies,
    int DisplayOrder,
    bool IsFeatured,
    bool IsVisible) : IRequest<Result<ProjectDto>>;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProjectDto>> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
        {
            return Result<ProjectDto>.Failure($"Project with ID '{request.Id}' was not found.");
        }

        project.UpdateDetails(
            title: request.Title,
            description: request.Description,
            image: request.Image,
            displayOrder: request.DisplayOrder,
            isFeatured: request.IsFeatured,
            isVisible: request.IsVisible);

        project.UpdateLinks(
            liveUrl: request.LiveUrl,
            githubUrl: request.GithubUrl);

        var targetTechs = (request.Technologies ?? [])
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .ToList();

        var toRemove = project.Technologies
            .Where(t => !targetTechs.Any(tt => string.Equals(tt, t, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var tech in toRemove)
        {
            project.RemoveTechnology(tech);
        }

        foreach (var tech in targetTechs)
        {
            project.AddTechnology(tech);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<ProjectDto>.Success(
            ProjectDto.FromEntity(project),
            "Project updated successfully.");
    }
}
