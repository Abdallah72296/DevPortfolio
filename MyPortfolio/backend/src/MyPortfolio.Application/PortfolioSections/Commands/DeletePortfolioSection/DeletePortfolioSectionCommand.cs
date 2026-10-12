using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.PortfolioSections.Commands.DeletePortfolioSection;

public record DeletePortfolioSectionCommand(Guid Id) : IRequest<Result>;

public class DeletePortfolioSectionCommandHandler : IRequestHandler<DeletePortfolioSectionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeletePortfolioSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeletePortfolioSectionCommand request,
        CancellationToken cancellationToken)
    {
        var section = await _context.PortfolioSections
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (section == null)
        {
            return Result.Failure($"Portfolio section with ID '{request.Id}' was not found.");
        }

        // Domain protection: system/default sections can never be permanently deleted
        if (!section.CanDelete())
        {
            return Result.Failure("System sections cannot be permanently deleted.");
        }

        _context.PortfolioSections.Remove(section);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success($"Portfolio section '{section.Title}' was permanently deleted.");
    }
}
