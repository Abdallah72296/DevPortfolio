using FluentValidation;

namespace MyPortfolio.Application.ContactMessages.Commands.MarkContactMessageAsUnread;

public class MarkContactMessageAsUnreadCommandValidator : AbstractValidator<MarkContactMessageAsUnreadCommand>
{
    public MarkContactMessageAsUnreadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Contact message ID is required.");
    }
}
