using FluentValidation;

namespace MyPortfolio.Application.Skills.Skills.Commands.SetSkillVisibility;

public class SetSkillVisibilityCommandValidator : AbstractValidator<SetSkillVisibilityCommand>
{
    public SetSkillVisibilityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Skill ID is required.");
    }
}
