using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Skills.Skills.Commands.DeleteSkill;

public record DeleteSkillCommand(Guid Id) : IRequest<Result>;

public class DeleteSkillCommandHandler : IRequestHandler<DeleteSkillCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteSkillCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteSkillCommand request,
        CancellationToken cancellationToken)
    {
        var skill = await _context.Skills
            .Include(s => s.SkillCategory)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (skill == null)
        {
            return Result.Failure($"Skill with ID '{request.Id}' was not found.");
        }

        // Clean up in-memory category association if attached
        skill.SkillCategory?.RemoveSkill(skill);

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success($"Skill '{skill.Name}' was deleted successfully.");
    }
}
