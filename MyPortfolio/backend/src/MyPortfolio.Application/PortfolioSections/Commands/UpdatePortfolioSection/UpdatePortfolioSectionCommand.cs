using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.PortfolioSections.Models;

namespace MyPortfolio.Application.PortfolioSections.Commands.UpdatePortfolioSection;

public record UpdatePortfolioSectionCommand(
    Guid Id,
    string Title,
    int DisplayOrder) : IRequest<Result<PortfolioSectionDto>>;

public class UpdatePortfolioSectionCommandHandler : IRequestHandler<UpdatePortfolioSectionCommand, Result<PortfolioSectionDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdatePortfolioSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PortfolioSectionDto>> Handle(
        UpdatePortfolioSectionCommand request,
        CancellationToken cancellationToken)
    {
        var section = await _context.PortfolioSections
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (section == null)
        {
            return Result<PortfolioSectionDto>.Failure($"Portfolio section with ID '{request.Id}' was not found.");
        }

        // Domain method updates title and displayOrder while preserving Key, IsVisible, and IsSystemSection
        section.UpdateDetails(request.Title, request.DisplayOrder, section.IsVisible);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<PortfolioSectionDto>.Success(
            PortfolioSectionDto.FromEntity(section),
            "Portfolio section updated successfully.");
    }
}
