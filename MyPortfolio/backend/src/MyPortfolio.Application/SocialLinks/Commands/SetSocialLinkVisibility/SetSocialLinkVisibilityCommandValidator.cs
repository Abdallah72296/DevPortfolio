using FluentValidation;

namespace MyPortfolio.Application.SocialLinks.Commands.SetSocialLinkVisibility;

public class SetSocialLinkVisibilityCommandValidator : AbstractValidator<SetSocialLinkVisibilityCommand>
{
    public SetSocialLinkVisibilityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Social link ID is required.");
    }
}
