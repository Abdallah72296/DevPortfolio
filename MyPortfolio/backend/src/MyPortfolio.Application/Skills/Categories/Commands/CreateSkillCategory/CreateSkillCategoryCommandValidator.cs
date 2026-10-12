using FluentValidation;

namespace MyPortfolio.Application.Skills.Categories.Commands.CreateSkillCategory;

public class CreateSkillCategoryCommandValidator : AbstractValidator<CreateSkillCategoryCommand>
{
    public CreateSkillCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
    }
}
