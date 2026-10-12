using FluentValidation;

namespace MyPortfolio.Application.CV.Commands.ParseCv;

public class ParseCvCommandValidator : AbstractValidator<ParseCvCommand>
{
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024; // 10 MB

    public ParseCvCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File stream is required.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required.")
            .Must(name => name != null && name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only PDF files are supported.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required.")
            .Must(type => string.Equals(type, "application/pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Content type must be 'application/pdf'.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("Uploaded file is empty.")
            .LessThanOrEqualTo(MaxFileSizeInBytes)
            .WithMessage($"File size exceeds maximum allowed size of {MaxFileSizeInBytes / (1024 * 1024)} MB.");
    }
}
