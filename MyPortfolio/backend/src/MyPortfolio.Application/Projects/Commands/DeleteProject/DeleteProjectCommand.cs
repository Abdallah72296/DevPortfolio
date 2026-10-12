using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid Id) : IRequest<Result>;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
        {
            return Result.Failure($"Project with ID '{request.Id}' was not found.");
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success($"Project '{project.Title}' deleted successfully.");
    }
}
