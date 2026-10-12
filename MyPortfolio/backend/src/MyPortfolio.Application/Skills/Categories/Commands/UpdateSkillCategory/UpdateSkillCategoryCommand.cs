using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;

namespace MyPortfolio.Application.Skills.Categories.Commands.UpdateSkillCategory;

public record UpdateSkillCategoryCommand(
    Guid Id,
    string Name,
    int DisplayOrder) : IRequest<Result<SkillCategoryDto>>;

public class UpdateSkillCategoryCommandHandler : IRequestHandler<UpdateSkillCategoryCommand, Result<SkillCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateSkillCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SkillCategoryDto>> Handle(
        UpdateSkillCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _context.SkillCategories
            .Include(c => c.Skills)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return Result<SkillCategoryDto>.Failure($"Skill category with ID '{request.Id}' was not found.");
        }

        category.UpdateDetails(request.Name, request.DisplayOrder, category.IsVisible);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<SkillCategoryDto>.Success(
            SkillCategoryDto.FromEntity(category),
            "Skill category updated successfully.");
    }
}
