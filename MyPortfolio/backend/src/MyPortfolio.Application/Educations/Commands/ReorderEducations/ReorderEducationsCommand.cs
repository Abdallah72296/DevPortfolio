using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Educations.Commands.ReorderEducations;

public record EducationOrderItem(Guid Id, int DisplayOrder);

public record ReorderEducationsCommand(List<EducationOrderItem> Items) : IRequest<Result>;

public class ReorderEducationsCommandHandler : IRequestHandler<ReorderEducationsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReorderEducationsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        ReorderEducationsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Failure("The reorder items list cannot be empty.");
        }

        var distinctIds = request.Items.Select(x => x.Id).Distinct().ToList();
        if (distinctIds.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate education IDs were found in the reorder request.");
        }

        var distinctOrders = request.Items.Select(x => x.DisplayOrder).Distinct().ToList();
        if (distinctOrders.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate display order values are not allowed in the reorder request.");
        }

        var educations = await _context.Educations
            .Where(e => distinctIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        if (educations.Count != distinctIds.Count)
        {
            return Result.Failure("One or more referenced education IDs were not found.");
        }

        var educationLookup = educations.ToDictionary(e => e.Id);
        foreach (var item in request.Items)
        {
            educationLookup[item.Id].UpdateDisplayOrder(item.DisplayOrder);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Educations reordered successfully.");
    }
}
