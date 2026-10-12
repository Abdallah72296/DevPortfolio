using FluentValidation;

namespace MyPortfolio.Application.ContactMessages.Commands.DeleteContactMessage;

public class DeleteContactMessageCommandValidator : AbstractValidator<DeleteContactMessageCommand>
{
    public DeleteContactMessageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Contact message ID is required.");
    }
}
