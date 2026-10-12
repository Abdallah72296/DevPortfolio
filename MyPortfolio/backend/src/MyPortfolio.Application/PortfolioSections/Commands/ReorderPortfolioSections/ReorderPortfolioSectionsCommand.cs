using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.PortfolioSections.Commands.ReorderPortfolioSections;

public record PortfolioSectionOrderItem(Guid Id, int DisplayOrder);

public record ReorderPortfolioSectionsCommand(List<PortfolioSectionOrderItem> Items) : IRequest<Result>;

public class ReorderPortfolioSectionsCommandHandler : IRequestHandler<ReorderPortfolioSectionsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReorderPortfolioSectionsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        ReorderPortfolioSectionsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Failure("The reorder items list cannot be empty.");
        }

        // Prevent duplicate IDs in the request
        var distinctIds = request.Items.Select(x => x.Id).Distinct().ToList();
        if (distinctIds.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate section IDs were found in the reorder request.");
        }

        // Prevent duplicate DisplayOrder values in the request
        var distinctOrders = request.Items.Select(x => x.DisplayOrder).Distinct().ToList();
        if (distinctOrders.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate display order values are not allowed.");
        }

        // Validate that all referenced section IDs exist
        var sections = await _context.PortfolioSections
            .Where(s => distinctIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        if (sections.Count != distinctIds.Count)
        {
            return Result.Failure("One or more referenced portfolio sections were not found.");
        }

        // Update each section through domain method
        var sectionLookup = sections.ToDictionary(s => s.Id);
        foreach (var item in request.Items)
        {
            sectionLookup[item.Id].UpdateDisplayOrder(item.DisplayOrder);
        }

        // Save all changes in one single atomic operation
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Portfolio sections reordered successfully.");
    }
}
