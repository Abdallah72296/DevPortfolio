using FluentValidation;

namespace MyPortfolio.Application.Experiences.Commands.SetExperienceVisibility;

public class SetExperienceVisibilityCommandValidator : AbstractValidator<SetExperienceVisibilityCommand>
{
    public SetExperienceVisibilityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Experience ID is required.");
    }
}
