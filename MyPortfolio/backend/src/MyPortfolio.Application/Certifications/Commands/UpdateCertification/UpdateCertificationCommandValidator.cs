using FluentValidation;

namespace MyPortfolio.Application.Certifications.Commands.UpdateCertification;

public class UpdateCertificationCommandValidator : AbstractValidator<UpdateCertificationCommand>
{
    public UpdateCertificationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Certification ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Certification name is required.")
            .MaximumLength(200).WithMessage("Certification name must not exceed 200 characters.");

        RuleFor(x => x.IssuingOrganization)
            .NotEmpty().WithMessage("Issuing organization is required.")
            .MaximumLength(200).WithMessage("Issuing organization must not exceed 200 characters.");

        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Issue date is required.");

        RuleFor(x => x.CredentialId)
            .MaximumLength(150).WithMessage("Credential ID must not exceed 150 characters.");

        RuleFor(x => x.CredentialUrl)
            .MaximumLength(500).WithMessage("Credential URL must not exceed 500 characters.")
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Credential URL must be a valid absolute URL.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
    }
}
