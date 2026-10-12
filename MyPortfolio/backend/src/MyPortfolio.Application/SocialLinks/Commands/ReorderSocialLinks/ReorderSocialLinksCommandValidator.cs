using FluentValidation;

namespace MyPortfolio.Application.SocialLinks.Commands.ReorderSocialLinks;

public class ReorderSocialLinksCommandValidator : AbstractValidator<ReorderSocialLinksCommand>
{
    public ReorderSocialLinksCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Reorder items list cannot be empty.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id)
                .NotEmpty().WithMessage("Social link ID is required.");

            item.RuleFor(i => i.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
        });
    }
}
