using FluentValidation;

namespace MyPortfolio.Application.ContactMessages.Commands.CreateContactMessage;

public class CreateContactMessageCommandValidator : AbstractValidator<CreateContactMessageCommand>
{
    public CreateContactMessageCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Sender name is required.")
            .MaximumLength(150).WithMessage("Sender name must not exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Sender email is required.")
            .MaximumLength(250).WithMessage("Sender email must not exceed 250 characters.")
            .EmailAddress().WithMessage("Sender email must be a valid email address.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Message subject is required.")
            .MaximumLength(250).WithMessage("Message subject must not exceed 250 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message content is required.")
            .MaximumLength(4000).WithMessage("Message content must not exceed 4000 characters.");
    }
}
