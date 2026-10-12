using FluentValidation;

namespace MyPortfolio.Application.PortfolioSections.Commands.DeletePortfolioSection;

public class DeletePortfolioSectionCommandValidator : AbstractValidator<DeletePortfolioSectionCommand>
{
    public DeletePortfolioSectionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Section ID is required.");
    }
}
