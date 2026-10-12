using FluentValidation;

namespace MyPortfolio.Application.PortfolioSections.Commands.CreatePortfolioSection;

public class CreatePortfolioSectionCommandValidator : AbstractValidator<CreatePortfolioSectionCommand>
{
    private static readonly HashSet<string> ReservedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "hero",
        "about",
        "skills",
        "experience",
        "projects",
        "education",
        "certifications",
        "contact"
    };

    public CreatePortfolioSectionCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Section key is required.")
            .MaximumLength(100).WithMessage("Section key must not exceed 100 characters.")
            .Must(key => string.IsNullOrWhiteSpace(key) || !ReservedKeys.Contains(key.Trim()))
            .WithMessage("The specified key is reserved for system sections.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Section title is required.")
            .MaximumLength(200).WithMessage("Section title must not exceed 200 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
    }
}
