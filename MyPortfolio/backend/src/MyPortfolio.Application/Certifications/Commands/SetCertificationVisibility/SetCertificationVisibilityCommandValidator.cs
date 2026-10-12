using FluentValidation;

namespace MyPortfolio.Application.Certifications.Commands.SetCertificationVisibility;

public class SetCertificationVisibilityCommandValidator : AbstractValidator<SetCertificationVisibilityCommand>
{
    public SetCertificationVisibilityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Certification ID is required.");
    }
}
