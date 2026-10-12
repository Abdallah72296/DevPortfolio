using FluentValidation;

namespace MyPortfolio.Application.Educations.Commands.DeleteEducation;

public class DeleteEducationCommandValidator : AbstractValidator<DeleteEducationCommand>
{
    public DeleteEducationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Education ID is required.");
    }
}
