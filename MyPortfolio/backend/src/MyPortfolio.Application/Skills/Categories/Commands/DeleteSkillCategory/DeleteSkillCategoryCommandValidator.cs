using FluentValidation;

namespace MyPortfolio.Application.Skills.Categories.Commands.DeleteSkillCategory;

public class DeleteSkillCategoryCommandValidator : AbstractValidator<DeleteSkillCategoryCommand>
{
    public DeleteSkillCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}
