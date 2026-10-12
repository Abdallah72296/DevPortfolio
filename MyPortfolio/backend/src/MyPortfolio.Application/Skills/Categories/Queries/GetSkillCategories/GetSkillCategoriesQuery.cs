using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;

namespace MyPortfolio.Application.Skills.Categories.Queries.GetSkillCategories;

public record GetSkillCategoriesQuery : IRequest<Result<List<SkillCategoryDto>>>;

public class GetSkillCategoriesQueryHandler : IRequestHandler<GetSkillCategoriesQuery, Result<List<SkillCategoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSkillCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SkillCategoryDto>>> Handle(
        GetSkillCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _context.SkillCategories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new SkillCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                DisplayOrder = c.DisplayOrder,
                IsVisible = c.IsVisible,
                Skills = c.Skills
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new SkillDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Icon = s.Icon,
                        DisplayOrder = s.DisplayOrder,
                        IsVisible = s.IsVisible,
                        SkillCategoryId = s.SkillCategoryId
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return Result<List<SkillCategoryDto>>.Success(categories);
    }
}
