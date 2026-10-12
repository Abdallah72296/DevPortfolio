using FluentValidation;

namespace MyPortfolio.Application.Skills.Skills.Commands.CreateSkill;

public class CreateSkillCommandValidator : AbstractValidator<CreateSkillCommand>
{
    public CreateSkillCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Skill name is required.")
            .MaximumLength(100).WithMessage("Skill name must not exceed 100 characters.");

        RuleFor(x => x.Icon)
            .MaximumLength(200).WithMessage("Icon must not exceed 200 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");

        RuleFor(x => x.SkillCategoryId)
            .NotEmpty().WithMessage("Skill category ID is required.");
    }
}
