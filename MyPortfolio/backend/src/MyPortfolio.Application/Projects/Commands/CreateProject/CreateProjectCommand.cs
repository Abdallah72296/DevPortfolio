using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Projects.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Projects.Commands.CreateProject;

public record CreateProjectCommand(
    string Title,
    string Description,
    string? Image,
    string? LiveUrl,
    string? GithubUrl,
    List<string>? Technologies,
    int DisplayOrder,
    bool IsFeatured) : IRequest<Result<ProjectDto>>;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProjectDto>> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = new Project(
            title: request.Title,
            description: request.Description,
            image: request.Image,
            liveUrl: request.LiveUrl,
            githubUrl: request.GithubUrl,
            displayOrder: request.DisplayOrder,
            isFeatured: request.IsFeatured,
            isVisible: true);

        if (request.Technologies != null)
        {
            foreach (var tech in request.Technologies)
            {
                if (!string.IsNullOrWhiteSpace(tech))
                {
                    project.AddTechnology(tech);
                }
            }
        }

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ProjectDto>.Success(
            ProjectDto.FromEntity(project),
            "Project created successfully.");
    }
}
