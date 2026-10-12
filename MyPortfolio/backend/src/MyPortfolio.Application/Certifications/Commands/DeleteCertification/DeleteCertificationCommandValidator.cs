using FluentValidation;

namespace MyPortfolio.Application.Certifications.Commands.DeleteCertification;

public class DeleteCertificationCommandValidator : AbstractValidator<DeleteCertificationCommand>
{
    public DeleteCertificationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Certification ID is required.");
    }
}
