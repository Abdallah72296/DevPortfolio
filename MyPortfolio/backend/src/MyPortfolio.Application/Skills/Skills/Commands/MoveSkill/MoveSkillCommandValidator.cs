using FluentValidation;

namespace MyPortfolio.Application.Skills.Skills.Commands.MoveSkill;

public class MoveSkillCommandValidator : AbstractValidator<MoveSkillCommand>
{
    public MoveSkillCommandValidator()
    {
        RuleFor(x => x.SkillId)
            .NotEmpty().WithMessage("Skill ID is required.");

        RuleFor(x => x.TargetSkillCategoryId)
            .NotEmpty().WithMessage("Target skill category ID is required.");
    }
}
