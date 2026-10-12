using FluentValidation;

namespace MyPortfolio.Application.ContactMessages.Commands.MarkContactMessageAsRead;

public class MarkContactMessageAsReadCommandValidator : AbstractValidator<MarkContactMessageAsReadCommand>
{
    public MarkContactMessageAsReadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Contact message ID is required.");
    }
}
