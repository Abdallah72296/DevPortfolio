using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.PortfolioSections.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.PortfolioSections.Commands.CreatePortfolioSection;

public record CreatePortfolioSectionCommand(
    string Key,
    string Title,
    int DisplayOrder) : IRequest<Result<PortfolioSectionDto>>;

public class CreatePortfolioSectionCommandHandler : IRequestHandler<CreatePortfolioSectionCommand, Result<PortfolioSectionDto>>
{
    private static readonly HashSet<string> ReservedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "hero",
        "about",
        "skills",
        "experience",
        "projects",
        "education",
        "certifications",
        "contact"
    };

    private readonly IApplicationDbContext _context;

    public CreatePortfolioSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PortfolioSectionDto>> Handle(
        CreatePortfolioSectionCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedKey = request.Key.Trim().ToLowerInvariant();

        if (ReservedKeys.Contains(normalizedKey))
        {
            return Result<PortfolioSectionDto>.Failure($"The section key '{normalizedKey}' is reserved for system sections.");
        }

        var keyExists = await _context.PortfolioSections
            .AnyAsync(s => s.Key == normalizedKey, cancellationToken);

        if (keyExists)
        {
            return Result<PortfolioSectionDto>.Failure($"A section with the key '{normalizedKey}' already exists.");
        }

        var section = new PortfolioSection(
            key: normalizedKey,
            title: request.Title,
            displayOrder: request.DisplayOrder,
            isVisible: true,
            isSystemSection: false);

        _context.PortfolioSections.Add(section);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<PortfolioSectionDto>.Success(
            PortfolioSectionDto.FromEntity(section),
            "Custom portfolio section created successfully.");
    }
}
