using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Skills.Categories.Commands.ReorderSkillCategories;

public record SkillCategoryOrderItem(Guid Id, int DisplayOrder);

public record ReorderSkillCategoriesCommand(List<SkillCategoryOrderItem> Items) : IRequest<Result>;

public class ReorderSkillCategoriesCommandHandler : IRequestHandler<ReorderSkillCategoriesCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReorderSkillCategoriesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        ReorderSkillCategoriesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Failure("The reorder items list cannot be empty.");
        }

        var distinctIds = request.Items.Select(x => x.Id).Distinct().ToList();
        if (distinctIds.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate category IDs were found in the reorder request.");
        }

        var distinctOrders = request.Items.Select(x => x.DisplayOrder).Distinct().ToList();
        if (distinctOrders.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate display order values are not allowed in the reorder request.");
        }

        var categories = await _context.SkillCategories
            .Where(c => distinctIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (categories.Count != distinctIds.Count)
        {
            return Result.Failure("One or more referenced skill category IDs were not found.");
        }

        var categoryLookup = categories.ToDictionary(c => c.Id);
        foreach (var item in request.Items)
        {
            categoryLookup[item.Id].UpdateDisplayOrder(item.DisplayOrder);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Skill categories reordered successfully.");
    }
}
