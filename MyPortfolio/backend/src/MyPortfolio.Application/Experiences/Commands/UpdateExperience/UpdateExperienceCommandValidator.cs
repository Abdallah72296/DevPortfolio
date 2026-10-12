using FluentValidation;

namespace MyPortfolio.Application.Experiences.Commands.UpdateExperience;

public class UpdateExperienceCommandValidator : AbstractValidator<UpdateExperienceCommand>
{
    public UpdateExperienceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Experience ID is required.");

        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(100).WithMessage("Job title must not exceed 100 characters.");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(100).WithMessage("Company name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");

        RuleFor(x => x)
            .Must(x => !x.IsCurrent || !x.EndDate.HasValue)
            .WithMessage("A current experience cannot have an end date.")
            .Must(x => x.IsCurrent || !x.EndDate.HasValue || x.EndDate.Value >= x.StartDate)
            .WithMessage("End date cannot precede the start date.");
    }
}
