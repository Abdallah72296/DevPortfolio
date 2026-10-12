using FluentValidation;

namespace MyPortfolio.Application.ContactMessages.Queries.GetContactMessageById;

public class GetContactMessageByIdQueryValidator : AbstractValidator<GetContactMessageByIdQuery>
{
    public GetContactMessageByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Contact message ID is required.");
    }
}
