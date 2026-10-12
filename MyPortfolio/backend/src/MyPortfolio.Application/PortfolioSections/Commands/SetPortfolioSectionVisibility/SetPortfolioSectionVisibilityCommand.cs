using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.PortfolioSections.Models;

namespace MyPortfolio.Application.PortfolioSections.Commands.SetPortfolioSectionVisibility;

public record SetPortfolioSectionVisibilityCommand(
    Guid Id,
    bool IsVisible) : IRequest<Result<PortfolioSectionDto>>;

public class SetPortfolioSectionVisibilityCommandHandler : IRequestHandler<SetPortfolioSectionVisibilityCommand, Result<PortfolioSectionDto>>
{
    private readonly IApplicationDbContext _context;

    public SetPortfolioSectionVisibilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PortfolioSectionDto>> Handle(
        SetPortfolioSectionVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var section = await _context.PortfolioSections
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (section == null)
        {
            return Result<PortfolioSectionDto>.Failure($"Portfolio section with ID '{request.Id}' was not found.");
        }

        // Domain method sets visibility and marks updated
        section.SetVisibility(request.IsVisible);

        await _context.SaveChangesAsync(cancellationToken);

        var statusMessage = request.IsVisible ? "visible" : "hidden";
        return Result<PortfolioSectionDto>.Success(
            PortfolioSectionDto.FromEntity(section),
            $"Portfolio section '{section.Title}' visibility set to {statusMessage}.");
    }
}
