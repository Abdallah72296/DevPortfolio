using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Projects.Commands.ReorderProjects;

public record ProjectOrderItem(Guid Id, int DisplayOrder);

public record ReorderProjectsCommand(List<ProjectOrderItem> Items) : IRequest<Result>;

public class ReorderProjectsCommandHandler : IRequestHandler<ReorderProjectsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReorderProjectsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        ReorderProjectsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Failure("The reorder items list cannot be empty.");
        }

        var distinctIds = request.Items.Select(x => x.Id).Distinct().ToList();
        if (distinctIds.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate project IDs were found in the reorder request.");
        }

        var distinctOrders = request.Items.Select(x => x.DisplayOrder).Distinct().ToList();
        if (distinctOrders.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate display order values are not allowed in the reorder request.");
        }

        var projects = await _context.Projects
            .Where(p => distinctIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (projects.Count != distinctIds.Count)
        {
            return Result.Failure("One or more referenced project IDs were not found.");
        }

        var projectLookup = projects.ToDictionary(p => p.Id);
        foreach (var item in request.Items)
        {
            projectLookup[item.Id].UpdateDisplayOrder(item.DisplayOrder);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Projects reordered successfully.");
    }
}
