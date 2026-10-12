using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;

namespace MyPortfolio.Application.Skills.Skills.Commands.SetSkillVisibility;

public record SetSkillVisibilityCommand(
    Guid Id,
    bool IsVisible) : IRequest<Result<SkillDto>>;

public class SetSkillVisibilityCommandHandler : IRequestHandler<SetSkillVisibilityCommand, Result<SkillDto>>
{
    private readonly IApplicationDbContext _context;

    public SetSkillVisibilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SkillDto>> Handle(
        SetSkillVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (skill == null)
        {
            return Result<SkillDto>.Failure($"Skill with ID '{request.Id}' was not found.");
        }

        skill.SetVisibility(request.IsVisible);

        await _context.SaveChangesAsync(cancellationToken);

        var status = request.IsVisible ? "visible" : "hidden";
        return Result<SkillDto>.Success(
            SkillDto.FromEntity(skill),
            $"Skill '{skill.Name}' visibility set to {status}.");
    }
}
