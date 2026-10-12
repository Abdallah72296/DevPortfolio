using FluentValidation;

namespace MyPortfolio.Application.Educations.Commands.ReorderEducations;

public class ReorderEducationsCommandValidator : AbstractValidator<ReorderEducationsCommand>
{
    public ReorderEducationsCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Reorder items list cannot be empty.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id)
                .NotEmpty().WithMessage("Education ID is required.");

            item.RuleFor(i => i.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
        });
    }
}
