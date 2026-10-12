using MediatR;
using MyPortfolio.Application.Common.Helpers;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Skills.Categories.Commands.CreateCategoryWithSkills;

public record CreateCategoryWithSkillsCommand(
    string Name,
    int DisplayOrder,
    List<string>? Skills) : IRequest<Result<SkillCategoryDto>>;

public class CreateCategoryWithSkillsCommandHandler : IRequestHandler<CreateCategoryWithSkillsCommand, Result<SkillCategoryDto>>
{
    private readonly IGenericRepository<SkillCategory> _categoryRepository;
    private readonly IGenericRepository<Skill> _skillRepository;

    public CreateCategoryWithSkillsCommandHandler(
        IGenericRepository<SkillCategory> categoryRepository,
        IGenericRepository<Skill> skillRepository)
    {
        _categoryRepository = categoryRepository;
        _skillRepository = skillRepository;
    }

    public async Task<Result<SkillCategoryDto>> Handle(CreateCategoryWithSkillsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<SkillCategoryDto>.Failure("Category name is required.");
        }

        var category = new SkillCategory(request.Name, request.DisplayOrder);
        await _categoryRepository.AddAsync(category, cancellationToken);

        var addedSkills = new List<Skill>();
        int order = 0;
        if (request.Skills != null)
        {
            foreach (var rawName in request.Skills)
            {
                var skillName = rawName?.Trim();
                if (string.IsNullOrWhiteSpace(skillName)) continue;

                var icon = SkillIconResolver.Resolve(skillName);
                var skill = category.AddSkill(skillName, icon, order++);
                addedSkills.Add(skill);
                await _skillRepository.AddAsync(skill, cancellationToken);
            }
        }

        await _categoryRepository.SaveChangesAsync(cancellationToken);

        return Result<SkillCategoryDto>.Success(
            SkillCategoryDto.FromEntity(category),
            $"Category '{category.Name}' created with {addedSkills.Count} skills.");
    }
}
