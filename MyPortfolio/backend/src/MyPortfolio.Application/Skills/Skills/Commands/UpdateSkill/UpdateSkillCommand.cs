using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;

namespace MyPortfolio.Application.Skills.Skills.Commands.UpdateSkill;

public record UpdateSkillCommand(
    Guid Id,
    string Name,
    string? Icon,
    int DisplayOrder) : IRequest<Result<SkillDto>>;

public class UpdateSkillCommandHandler : IRequestHandler<UpdateSkillCommand, Result<SkillDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateSkillCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SkillDto>> Handle(
        UpdateSkillCommand request,
        CancellationToken cancellationToken)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (skill == null)
        {
            return Result<SkillDto>.Failure($"Skill with ID '{request.Id}' was not found.");
        }

        // Domain method updates details preserving IsVisible and SkillCategoryId
        skill.UpdateDetails(request.Name, request.Icon, request.DisplayOrder, skill.IsVisible);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<SkillDto>.Success(
            SkillDto.FromEntity(skill),
            "Skill updated successfully.");
    }
}
