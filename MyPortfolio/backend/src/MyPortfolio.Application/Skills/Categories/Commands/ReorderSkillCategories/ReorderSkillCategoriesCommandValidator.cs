using FluentValidation;

namespace MyPortfolio.Application.Skills.Categories.Commands.ReorderSkillCategories;

public class ReorderSkillCategoriesCommandValidator : AbstractValidator<ReorderSkillCategoriesCommand>
{
    public ReorderSkillCategoriesCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("The reorder items list is required.")
            .NotEmpty().WithMessage("The reorder items list cannot be empty.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id)
                .NotEmpty().WithMessage("Category ID is required.");

            item.RuleFor(i => i.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
        });

        RuleFor(x => x.Items)
            .Must(items => items == null || items.Select(i => i.Id).Distinct().Count() == items.Count)
            .WithMessage("Duplicate category IDs are not allowed in the reorder request.")
            .Must(items => items == null || items.Select(i => i.DisplayOrder).Distinct().Count() == items.Count)
            .WithMessage("Duplicate display order values are not allowed in the reorder request.");
    }
}
