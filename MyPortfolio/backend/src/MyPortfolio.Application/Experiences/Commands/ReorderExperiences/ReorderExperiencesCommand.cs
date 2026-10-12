using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Experiences.Commands.ReorderExperiences;

public record ExperienceOrderItem(Guid Id, int DisplayOrder);

public record ReorderExperiencesCommand(List<ExperienceOrderItem> Items) : IRequest<Result>;

public class ReorderExperiencesCommandHandler : IRequestHandler<ReorderExperiencesCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReorderExperiencesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        ReorderExperiencesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Failure("The reorder items list cannot be empty.");
        }

        var distinctIds = request.Items.Select(x => x.Id).Distinct().ToList();
        if (distinctIds.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate experience IDs were found in the reorder request.");
        }

        var distinctOrders = request.Items.Select(x => x.DisplayOrder).Distinct().ToList();
        if (distinctOrders.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate display order values are not allowed in the reorder request.");
        }

        var experiences = await _context.Experiences
            .Where(e => distinctIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        if (experiences.Count != distinctIds.Count)
        {
            return Result.Failure("One or more referenced experience IDs were not found.");
        }

        var experienceLookup = experiences.ToDictionary(e => e.Id);
        foreach (var item in request.Items)
        {
            experienceLookup[item.Id].UpdateDisplayOrder(item.DisplayOrder);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Experiences reordered successfully.");
    }
}
