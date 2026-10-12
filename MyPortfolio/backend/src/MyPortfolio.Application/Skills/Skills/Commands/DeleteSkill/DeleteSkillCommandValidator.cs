using FluentValidation;

namespace MyPortfolio.Application.Skills.Skills.Commands.DeleteSkill;

public class DeleteSkillCommandValidator : AbstractValidator<DeleteSkillCommand>
{
    public DeleteSkillCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Skill ID is required.");
    }
}
