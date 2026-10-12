using FluentValidation;

namespace MyPortfolio.Application.SocialLinks.Commands.DeleteSocialLink;

public class DeleteSocialLinkCommandValidator : AbstractValidator<DeleteSocialLinkCommand>
{
    public DeleteSocialLinkCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Social link ID is required.");
    }
}
