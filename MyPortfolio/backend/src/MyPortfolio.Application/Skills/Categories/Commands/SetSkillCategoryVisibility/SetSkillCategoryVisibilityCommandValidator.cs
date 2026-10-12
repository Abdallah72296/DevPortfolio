using FluentValidation;

namespace MyPortfolio.Application.Skills.Categories.Commands.SetSkillCategoryVisibility;

public class SetSkillCategoryVisibilityCommandValidator : AbstractValidator<SetSkillCategoryVisibilityCommand>
{
    public SetSkillCategoryVisibilityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}
