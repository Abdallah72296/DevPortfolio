using FluentValidation;
using MyPortfolio.Application.CV.Models;

namespace MyPortfolio.Application.CV.Validation;

public class CvPortfolioDraftValidator : AbstractValidator<CvPortfolioDraft>
{
    public CvPortfolioDraftValidator()
    {
        When(x => x.PersonalInfo != null, () =>
        {
            RuleFor(x => x.PersonalInfo!.Name)
                .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

            RuleFor(x => x.PersonalInfo!.JobTitle)
                .MaximumLength(150).WithMessage("Job title must not exceed 150 characters.");

            RuleFor(x => x.PersonalInfo!.About)
                .MaximumLength(2000).WithMessage("About section must not exceed 2000 characters.");
        });

        RuleForEach(x => x.Skills).ChildRules(category =>
        {
            category.RuleFor(c => c.Category)
                .MaximumLength(100).WithMessage("Skill category name must not exceed 100 characters.");

            category.RuleForEach(c => c.Skills).ChildRules(skill =>
            {
                skill.RuleFor(s => s.Name)
                    .MaximumLength(100).WithMessage("Skill name must not exceed 100 characters.");
            });
        });

        RuleForEach(x => x.Experiences).ChildRules(exp =>
        {
            exp.RuleFor(e => e.JobTitle)
                .MaximumLength(150).WithMessage("Job title must not exceed 150 characters.");

            exp.RuleFor(e => e.CompanyName)
                .MaximumLength(150).WithMessage("Company name must not exceed 150 characters.");

            exp.RuleFor(e => e.Description)
                .MaximumLength(2000).WithMessage("Experience description must not exceed 2000 characters.");
        });

        RuleForEach(x => x.Projects).ChildRules(proj =>
        {
            proj.RuleFor(p => p.Title)
                .MaximumLength(200).WithMessage("Project title must not exceed 200 characters.");

            proj.RuleFor(p => p.Description)
                .MaximumLength(2000).WithMessage("Project description must not exceed 2000 characters.");

            proj.RuleFor(p => p.LiveUrl)
                .Must(BeAValidUrlOrEmpty)
                .WithMessage("Project live URL must be a valid absolute URL when supplied.");

            proj.RuleFor(p => p.GithubUrl)
                .Must(BeAValidUrlOrEmpty)
                .WithMessage("Project GitHub URL must be a valid absolute URL when supplied.");
        });

        RuleForEach(x => x.Education).ChildRules(edu =>
        {
            edu.RuleFor(e => e.InstitutionName)
                .MaximumLength(200).WithMessage("Institution name must not exceed 200 characters.");

            edu.RuleFor(e => e.Degree)
                .MaximumLength(150).WithMessage("Degree must not exceed 150 characters.");

            edu.RuleFor(e => e.FieldOfStudy)
                .MaximumLength(150).WithMessage("Field of study must not exceed 150 characters.");

            edu.RuleFor(e => e.Description)
                .MaximumLength(1000).WithMessage("Education description must not exceed 1000 characters.");
        });

        RuleForEach(x => x.Certifications).ChildRules(cert =>
        {
            cert.RuleFor(c => c.Name)
                .MaximumLength(200).WithMessage("Certification name must not exceed 200 characters.");

            cert.RuleFor(c => c.IssuingOrganization)
                .MaximumLength(200).WithMessage("Issuing organization must not exceed 200 characters.");

            cert.RuleFor(c => c.CredentialUrl)
                .Must(BeAValidUrlOrEmpty)
                .WithMessage("Credential URL must be a valid absolute URL when supplied.");

            cert.RuleFor(c => c.Description)
                .MaximumLength(1000).WithMessage("Certification description must not exceed 1000 characters.");
        });

        RuleForEach(x => x.SocialLinks).ChildRules(social =>
        {
            social.RuleFor(s => s.Platform)
                .MaximumLength(50).WithMessage("Platform must not exceed 50 characters.");

            social.RuleFor(s => s.Url)
                .Must(BeAValidUrlOrEmpty)
                .WithMessage("Social link URL must be a valid absolute URL when supplied.");
        });
    }

    private static bool BeAValidUrlOrEmpty(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
