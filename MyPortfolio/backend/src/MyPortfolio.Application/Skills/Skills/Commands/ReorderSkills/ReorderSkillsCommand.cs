using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Skills.Skills.Commands.ReorderSkills;

public record SkillOrderItem(Guid Id, int DisplayOrder);

public record ReorderSkillsCommand(
    Guid SkillCategoryId,
    List<SkillOrderItem> Items) : IRequest<Result>;

public class ReorderSkillsCommandHandler : IRequestHandler<ReorderSkillsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReorderSkillsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        ReorderSkillsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Failure("The reorder items list cannot be empty.");
        }

        var categoryExists = await _context.SkillCategories
            .AnyAsync(c => c.Id == request.SkillCategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure($"Skill category with ID '{request.SkillCategoryId}' was not found.");
        }

        var distinctIds = request.Items.Select(x => x.Id).Distinct().ToList();
        if (distinctIds.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate skill IDs were found in the reorder request.");
        }

        var distinctOrders = request.Items.Select(x => x.DisplayOrder).Distinct().ToList();
        if (distinctOrders.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate display order values are not allowed in the reorder request.");
        }

        var skills = await _context.Skills
            .Where(s => distinctIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        if (skills.Count != distinctIds.Count)
        {
            return Result.Failure("One or more referenced skill IDs were not found.");
        }

        // Prevent cross-category reordering: all skills must belong to the specified category
        var foreignSkills = skills.Where(s => s.SkillCategoryId != request.SkillCategoryId).ToList();
        if (foreignSkills.Count > 0)
        {
            return Result.Failure("All skills in a reorder request must belong to the specified category.");
        }

        var skillLookup = skills.ToDictionary(s => s.Id);
        foreach (var item in request.Items)
        {
            skillLookup[item.Id].UpdateDisplayOrder(item.DisplayOrder);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Skills reordered successfully.");
    }
}
