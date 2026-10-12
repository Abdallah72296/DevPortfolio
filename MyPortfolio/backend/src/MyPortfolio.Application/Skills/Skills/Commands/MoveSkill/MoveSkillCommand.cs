using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;

namespace MyPortfolio.Application.Skills.Skills.Commands.MoveSkill;

public record MoveSkillCommand(
    Guid SkillId,
    Guid TargetSkillCategoryId) : IRequest<Result<SkillDto>>;

public class MoveSkillCommandHandler : IRequestHandler<MoveSkillCommand, Result<SkillDto>>
{
    private readonly IApplicationDbContext _context;

    public MoveSkillCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SkillDto>> Handle(
        MoveSkillCommand request,
        CancellationToken cancellationToken)
    {
        var skill = await _context.Skills
            .Include(s => s.SkillCategory)
            .FirstOrDefaultAsync(s => s.Id == request.SkillId, cancellationToken);

        if (skill == null)
        {
            return Result<SkillDto>.Failure($"Skill with ID '{request.SkillId}' was not found.");
        }

        var targetCategory = await _context.SkillCategories
            .FirstOrDefaultAsync(c => c.Id == request.TargetSkillCategoryId, cancellationToken);

        if (targetCategory == null)
        {
            return Result<SkillDto>.Failure($"Target skill category with ID '{request.TargetSkillCategoryId}' was not found.");
        }

        // Domain method keeps in-memory category relationships consistent and updates SkillCategoryId
        skill.MoveToCategory(targetCategory);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<SkillDto>.Success(
            SkillDto.FromEntity(skill),
            $"Skill '{skill.Name}' moved to category '{targetCategory.Name}' successfully.");
    }
}
