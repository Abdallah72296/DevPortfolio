using FluentValidation;

namespace MyPortfolio.Application.Experiences.Commands.DeleteExperience;

public class DeleteExperienceCommandValidator : AbstractValidator<DeleteExperienceCommand>
{
    public DeleteExperienceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Experience ID is required.");
    }
}
