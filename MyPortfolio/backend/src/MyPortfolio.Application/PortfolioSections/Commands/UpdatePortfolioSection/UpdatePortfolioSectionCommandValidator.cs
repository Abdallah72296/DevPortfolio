using FluentValidation;

namespace MyPortfolio.Application.PortfolioSections.Commands.UpdatePortfolioSection;

public class UpdatePortfolioSectionCommandValidator : AbstractValidator<UpdatePortfolioSectionCommand>
{
    public UpdatePortfolioSectionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Section ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Section title is required.")
            .MaximumLength(200).WithMessage("Section title must not exceed 200 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
    }
}
