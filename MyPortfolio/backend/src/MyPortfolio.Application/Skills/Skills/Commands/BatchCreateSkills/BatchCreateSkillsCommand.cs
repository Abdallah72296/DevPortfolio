using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Helpers;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Skills.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Skills.Skills.Commands.BatchCreateSkills;

public record BatchCreateSkillsCommand(
    Guid SkillCategoryId,
    List<string> SkillNames,
    string? DefaultIcon = null) : IRequest<Result<List<SkillDto>>>;

public class BatchCreateSkillsCommandHandler : IRequestHandler<BatchCreateSkillsCommand, Result<List<SkillDto>>>
{
    private readonly IGenericRepository<SkillCategory> _categoryRepository;
    private readonly IGenericRepository<Skill> _skillRepository;

    public BatchCreateSkillsCommandHandler(
        IGenericRepository<SkillCategory> categoryRepository,
        IGenericRepository<Skill> skillRepository)
    {
        _categoryRepository = categoryRepository;
        _skillRepository = skillRepository;
    }

    public async Task<Result<List<SkillDto>>> Handle(BatchCreateSkillsCommand request, CancellationToken cancellationToken)
    {
        if (request.SkillNames == null || request.SkillNames.Count == 0)
        {
            return Result<List<SkillDto>>.Failure("No skills provided.");
        }

        var category = await _categoryRepository.GetByIdAsync(request.SkillCategoryId, cancellationToken);
        if (category == null)
        {
            return Result<List<SkillDto>>.Failure("Skill category not found.");
        }

        var existingSkills = await _skillRepository.FindAsync(
            s => s.SkillCategoryId == request.SkillCategoryId,
            cancellationToken);

        var currentMaxOrder = existingSkills.Count > 0 ? existingSkills.Max(s => s.DisplayOrder) : 0;
        var createdSkills = new List<SkillDto>();
        var mutableExisting = existingSkills.ToList();

        foreach (var rawName in request.SkillNames)
        {
            var name = rawName?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            if (mutableExisting.Any(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            currentMaxOrder++;
            var icon = !string.IsNullOrWhiteSpace(request.DefaultIcon)
                ? request.DefaultIcon
                : SkillIconResolver.Resolve(name);

            var skill = new Skill(name, category.Id, currentMaxOrder, icon);
            await _skillRepository.AddAsync(skill, cancellationToken);
            mutableExisting.Add(skill);

            createdSkills.Add(SkillDto.FromEntity(skill));
        }

        await _skillRepository.SaveChangesAsync(cancellationToken);
        return Result<List<SkillDto>>.Success(createdSkills, $"Created {createdSkills.Count} skills successfully.");
    }
}
