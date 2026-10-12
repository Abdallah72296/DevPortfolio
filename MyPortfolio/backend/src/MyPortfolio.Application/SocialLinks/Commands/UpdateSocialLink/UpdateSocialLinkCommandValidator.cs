using FluentValidation;

namespace MyPortfolio.Application.SocialLinks.Commands.UpdateSocialLink;

public class UpdateSocialLinkCommandValidator : AbstractValidator<UpdateSocialLinkCommand>
{
    public UpdateSocialLinkCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Social link ID is required.");

        RuleFor(x => x.Platform)
            .NotEmpty().WithMessage("Platform is required.")
            .MaximumLength(100).WithMessage("Platform must not exceed 100 characters.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required.")
            .MaximumLength(500).WithMessage("URL must not exceed 500 characters.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("URL must be a valid absolute URL.");

        RuleFor(x => x.Icon)
            .MaximumLength(200).WithMessage("Icon must not exceed 200 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
    }
}
