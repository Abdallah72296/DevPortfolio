using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.SocialLinks.Commands.ReorderSocialLinks;

public record SocialLinkOrderItem(Guid Id, int DisplayOrder);

public record ReorderSocialLinksCommand(List<SocialLinkOrderItem> Items) : IRequest<Result>;

public class ReorderSocialLinksCommandHandler : IRequestHandler<ReorderSocialLinksCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReorderSocialLinksCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        ReorderSocialLinksCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Failure("The reorder items list cannot be empty.");
        }

        var distinctIds = request.Items.Select(x => x.Id).Distinct().ToList();
        if (distinctIds.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate social link IDs were found in the reorder request.");
        }

        var distinctOrders = request.Items.Select(x => x.DisplayOrder).Distinct().ToList();
        if (distinctOrders.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate display order values are not allowed in the reorder request.");
        }

        var links = await _context.SocialLinks
            .Where(s => distinctIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        if (links.Count != distinctIds.Count)
        {
            return Result.Failure("One or more referenced social link IDs were not found.");
        }

        var linkLookup = links.ToDictionary(s => s.Id);
        foreach (var item in request.Items)
        {
            linkLookup[item.Id].UpdateDisplayOrder(item.DisplayOrder);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Social links reordered successfully.");
    }
}
