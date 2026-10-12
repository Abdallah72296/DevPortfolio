using FluentValidation;

namespace MyPortfolio.Application.PortfolioSections.Commands.SetPortfolioSectionVisibility;

public class SetPortfolioSectionVisibilityCommandValidator : AbstractValidator<SetPortfolioSectionVisibilityCommand>
{
    public SetPortfolioSectionVisibilityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Section ID is required.");
    }
}
