using FluentValidation;

namespace MyPortfolio.Application.Projects.Commands.SetProjectFeatured;

public class SetProjectFeaturedCommandValidator : AbstractValidator<SetProjectFeaturedCommand>
{
    public SetProjectFeaturedCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required.");
    }
}
