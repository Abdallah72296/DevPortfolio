using FluentValidation;

namespace MyPortfolio.Application.Educations.Commands.SetEducationVisibility;

public class SetEducationVisibilityCommandValidator : AbstractValidator<SetEducationVisibilityCommand>
{
    public SetEducationVisibilityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Education ID is required.");
    }
}
