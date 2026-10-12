using FluentValidation;

namespace MyPortfolio.Application.Projects.Commands.UpdateProject;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(200).WithMessage("Project title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Project description is required.")
            .MaximumLength(4000).WithMessage("Project description must not exceed 4000 characters.");

        RuleFor(x => x.Image)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.");

        RuleFor(x => x.LiveUrl)
            .MaximumLength(500).WithMessage("Live URL must not exceed 500 characters.")
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Live URL must be a valid absolute URL.");

        RuleFor(x => x.GithubUrl)
            .MaximumLength(500).WithMessage("GitHub URL must not exceed 500 characters.")
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("GitHub URL must be a valid absolute URL.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
    }
}
