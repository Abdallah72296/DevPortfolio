using FluentValidation;

namespace MyPortfolio.Application.Certifications.Commands.ReorderCertifications;

public class ReorderCertificationsCommandValidator : AbstractValidator<ReorderCertificationsCommand>
{
    public ReorderCertificationsCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Reorder items list cannot be empty.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id)
                .NotEmpty().WithMessage("Certification ID is required.");

            item.RuleFor(i => i.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
        });
    }
}
