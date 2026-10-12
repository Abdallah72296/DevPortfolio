using FluentValidation;

namespace MyPortfolio.Application.Educations.Commands.UpdateEducation;

public class UpdateEducationCommandValidator : AbstractValidator<UpdateEducationCommand>
{
    public UpdateEducationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Education ID is required.");

        RuleFor(x => x.InstitutionName)
            .NotEmpty().WithMessage("Institution name is required.")
            .MaximumLength(200).WithMessage("Institution name must not exceed 200 characters.");

        RuleFor(x => x.Degree)
            .NotEmpty().WithMessage("Degree is required.")
            .MaximumLength(150).WithMessage("Degree must not exceed 150 characters.");

        RuleFor(x => x.FieldOfStudy)
            .MaximumLength(150).WithMessage("Field of study must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");

        RuleFor(x => x)
            .Must(x => !x.IsCurrent || !x.EndDate.HasValue)
            .WithMessage("A current education cannot have an end date.")
            .Must(x => x.IsCurrent || !x.EndDate.HasValue || x.EndDate.Value >= x.StartDate)
            .WithMessage("End date cannot precede the start date.");
    }
}
