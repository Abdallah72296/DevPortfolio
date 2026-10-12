using FluentValidation;

namespace MyPortfolio.Application.Skills.Skills.Commands.ReorderSkills;

public class ReorderSkillsCommandValidator : AbstractValidator<ReorderSkillsCommand>
{
    public ReorderSkillsCommandValidator()
    {
        RuleFor(x => x.SkillCategoryId)
            .NotEmpty().WithMessage("Skill category ID is required.");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("The reorder items list is required.")
            .NotEmpty().WithMessage("The reorder items list cannot be empty.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id)
                .NotEmpty().WithMessage("Skill ID is required.");

            item.RuleFor(i => i.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
        });

        RuleFor(x => x.Items)
            .Must(items => items == null || items.Select(i => i.Id).Distinct().Count() == items.Count)
            .WithMessage("Duplicate skill IDs are not allowed in the reorder request.")
            .Must(items => items == null || items.Select(i => i.DisplayOrder).Distinct().Count() == items.Count)
            .WithMessage("Duplicate display order values are not allowed in the reorder request.");
    }
}
