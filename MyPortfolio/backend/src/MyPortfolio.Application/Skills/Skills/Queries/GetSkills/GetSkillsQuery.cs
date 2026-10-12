using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;

namespace MyPortfolio.Application.Skills.Skills.Queries.GetSkills;

public record GetSkillsQuery(Guid? SkillCategoryId = null) : IRequest<Result<List<SkillDto>>>;

public class GetSkillsQueryHandler : IRequestHandler<GetSkillsQuery, Result<List<SkillDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSkillsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SkillDto>>> Handle(
        GetSkillsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Skills.AsNoTracking();

        if (request.SkillCategoryId.HasValue && request.SkillCategoryId.Value != Guid.Empty)
        {
            query = query.Where(s => s.SkillCategoryId == request.SkillCategoryId.Value);
        }

        var skills = await query
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
            .ToListAsync(cancellationToken);

        return Result<List<SkillDto>>.Success(skills);
    }
}
