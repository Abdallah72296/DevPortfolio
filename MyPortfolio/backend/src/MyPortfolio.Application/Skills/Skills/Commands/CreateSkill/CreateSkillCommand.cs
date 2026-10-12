using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Skills.Skills.Commands.CreateSkill;

public record CreateSkillCommand(
    string Name,
    string? Icon,
    int DisplayOrder,
    Guid SkillCategoryId) : IRequest<Result<SkillDto>>;

public class CreateSkillCommandHandler : IRequestHandler<CreateSkillCommand, Result<SkillDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateSkillCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SkillDto>> Handle(
        CreateSkillCommand request,
        CancellationToken cancellationToken)
    {
        var categoryExists = await _context.SkillCategories
            .AnyAsync(c => c.Id == request.SkillCategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result<SkillDto>.Failure($"Skill category with ID '{request.SkillCategoryId}' was not found.");
        }

        var icon = !string.IsNullOrWhiteSpace(request.Icon)
            ? request.Icon
            : MyPortfolio.Application.Common.Helpers.SkillIconResolver.Resolve(request.Name);

        var skill = new Skill(
            name: request.Name,
            skillCategoryId: request.SkillCategoryId,
            displayOrder: request.DisplayOrder,
            icon: icon,
            isVisible: true);

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<SkillDto>.Success(
            SkillDto.FromEntity(skill),
            "Skill created successfully.");
    }
}
