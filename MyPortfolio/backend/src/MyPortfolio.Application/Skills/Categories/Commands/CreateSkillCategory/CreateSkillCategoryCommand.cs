using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Skills.Categories.Commands.CreateSkillCategory;

public record CreateSkillCategoryCommand(
    string Name,
    int DisplayOrder) : IRequest<Result<SkillCategoryDto>>;

public class CreateSkillCategoryCommandHandler : IRequestHandler<CreateSkillCategoryCommand, Result<SkillCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateSkillCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SkillCategoryDto>> Handle(
        CreateSkillCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = new SkillCategory(
            name: request.Name,
            displayOrder: request.DisplayOrder,
            isVisible: true);

        _context.SkillCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<SkillCategoryDto>.Success(
            SkillCategoryDto.FromEntity(category),
            "Skill category created successfully.");
    }
}
