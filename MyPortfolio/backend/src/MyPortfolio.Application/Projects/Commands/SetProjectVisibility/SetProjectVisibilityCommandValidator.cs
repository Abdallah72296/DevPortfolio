using FluentValidation;

namespace MyPortfolio.Application.Projects.Commands.SetProjectVisibility;

public class SetProjectVisibilityCommandValidator : AbstractValidator<SetProjectVisibilityCommand>
{
    public SetProjectVisibilityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required.");
    }
}
