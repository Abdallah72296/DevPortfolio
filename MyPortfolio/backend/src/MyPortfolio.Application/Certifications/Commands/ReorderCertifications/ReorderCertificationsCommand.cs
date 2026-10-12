using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Certifications.Commands.ReorderCertifications;

public record CertificationOrderItem(Guid Id, int DisplayOrder);

public record ReorderCertificationsCommand(List<CertificationOrderItem> Items) : IRequest<Result>;

public class ReorderCertificationsCommandHandler : IRequestHandler<ReorderCertificationsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReorderCertificationsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        ReorderCertificationsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Failure("The reorder items list cannot be empty.");
        }

        var distinctIds = request.Items.Select(x => x.Id).Distinct().ToList();
        if (distinctIds.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate certification IDs were found in the reorder request.");
        }

        var distinctOrders = request.Items.Select(x => x.DisplayOrder).Distinct().ToList();
        if (distinctOrders.Count != request.Items.Count)
        {
            return Result.Failure("Duplicate display order values are not allowed in the reorder request.");
        }

        var certifications = await _context.Certifications
            .Where(c => distinctIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (certifications.Count != distinctIds.Count)
        {
            return Result.Failure("One or more referenced certification IDs were not found.");
        }

        var certificationLookup = certifications.ToDictionary(c => c.Id);
        foreach (var item in request.Items)
        {
            certificationLookup[item.Id].UpdateDisplayOrder(item.DisplayOrder);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Certifications reordered successfully.");
    }
}
