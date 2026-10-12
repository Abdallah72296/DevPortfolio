using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;

namespace MyPortfolio.Application.Skills.Categories.Commands.SetSkillCategoryVisibility;

public record SetSkillCategoryVisibilityCommand(
    Guid Id,
    bool IsVisible) : IRequest<Result<SkillCategoryDto>>;

public class SetSkillCategoryVisibilityCommandHandler : IRequestHandler<SetSkillCategoryVisibilityCommand, Result<SkillCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public SetSkillCategoryVisibilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SkillCategoryDto>> Handle(
        SetSkillCategoryVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _context.SkillCategories
            .Include(c => c.Skills)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return Result<SkillCategoryDto>.Failure($"Skill category with ID '{request.Id}' was not found.");
        }

        category.SetVisibility(request.IsVisible);

        await _context.SaveChangesAsync(cancellationToken);

        var status = request.IsVisible ? "visible" : "hidden";
        return Result<SkillCategoryDto>.Success(
            SkillCategoryDto.FromEntity(category),
            $"Skill category '{category.Name}' visibility set to {status}.");
    }
}
