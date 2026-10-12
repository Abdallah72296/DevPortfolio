using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.CV.Interfaces;
using MyPortfolio.Application.CV.Models;

namespace MyPortfolio.Application.CV.Commands.ParseCv;

public class ParseCvCommandHandler : IRequestHandler<ParseCvCommand, Result<CvPortfolioDraft>>
{
    private static readonly byte[] PdfMagicBytes = "%PDF-"u8.ToArray();

    private readonly ICvParser _cvParser;
    private readonly IValidator<CvPortfolioDraft> _draftValidator;
    private readonly ILogger<ParseCvCommandHandler> _logger;

    public ParseCvCommandHandler(
        ICvParser cvParser,
        IValidator<CvPortfolioDraft> draftValidator,
        ILogger<ParseCvCommandHandler> logger)
    {
        _cvParser = cvParser;
        _draftValidator = draftValidator;
        _logger = logger;
    }

    public async Task<Result<CvPortfolioDraft>> Handle(
        ParseCvCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate PDF signature magic bytes (%PDF-)
        if (!IsValidPdfHeader(request.FileStream))
        {
            _logger.LogWarning("Uploaded file failed PDF magic bytes validation.");
            return Result<CvPortfolioDraft>.Failure("The uploaded file is not a valid PDF document.");
        }

        // 2. Reset stream position to beginning before passing to parser
        if (request.FileStream.CanSeek)
        {
            request.FileStream.Position = 0;
        }

        // 3. Delegate directly to ICvParser (Gemini processes the PDF document directly)
        var parseResult = await _cvParser.ParseCvAsync(request.FileStream, request.ContentType, cancellationToken);
        if (!parseResult.Succeeded || parseResult.Data == null)
        {
            _logger.LogWarning("AI CV Parser returned failure: {Message}", parseResult.Message);
            return Result<CvPortfolioDraft>.Failure(
                parseResult.Message ?? "Failed to extract structured portfolio data from the CV.",
                parseResult.Errors);
        }

        var draft = parseResult.Data;

        // 4. Normalize and sanitize draft data
        var normalizedDraft = NormalizeDraft(draft);

        // 5. Validate normalized draft
        var validationResult = await _draftValidator.ValidateAsync(normalizedDraft, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Parsed CV draft had validation issues: {Count} error(s).", errors.Count);
            return Result<CvPortfolioDraft>.Failure("The extracted CV data contained validation errors.", errors);
        }

        return Result<CvPortfolioDraft>.Success(normalizedDraft);
    }

    private static bool IsValidPdfHeader(Stream stream)
    {
        if (!stream.CanRead)
        {
            return false;
        }

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var header = new byte[PdfMagicBytes.Length];
        var bytesRead = stream.Read(header, 0, header.Length);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        if (bytesRead < PdfMagicBytes.Length)
        {
            return false;
        }

        for (int i = 0; i < PdfMagicBytes.Length; i++)
        {
            if (header[i] != PdfMagicBytes[i])
            {
                return false;
            }
        }

        return true;
    }

    private static CvPortfolioDraft NormalizeDraft(CvPortfolioDraft draft)
    {
        // 1. Personal Info sanitization
        var personalInfo = draft.PersonalInfo == null ? null : new CvPersonalInfoDraft(
            Clamp(draft.PersonalInfo.Name, 150),
            Clamp(draft.PersonalInfo.JobTitle, 150),
            Clamp(draft.PersonalInfo.About, 2000));

        // 2. Skills sanitization
        var skills = (draft.Skills ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c.Category))
            .Select(c => new CvSkillCategoryDraft(
                Clamp(c.Category.Trim(), 100)!,
                (c.Skills ?? [])
                    .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                    .Select(s => new CvSkillDraft(
                        Clamp(s.Name.Trim(), 100)!,
                        Clamp(s.Icon, 100)))
                    .ToList()))
            .ToList();

        // 3. Experiences sanitization
        var experiences = (draft.Experiences ?? [])
            .Where(e => !string.IsNullOrWhiteSpace(e.JobTitle) || !string.IsNullOrWhiteSpace(e.CompanyName))
            .Select(e => new CvExperienceDraft(
                Clamp(e.JobTitle.Trim(), 150) ?? "Professional",
                Clamp(e.CompanyName.Trim(), 150) ?? "Company",
                Clamp(e.Description, 2000),
                e.StartDate,
                e.IsCurrent ? null : e.EndDate,
                e.IsCurrent))
            .ToList();

        // 4. Projects sanitization
        var projects = (draft.Projects ?? [])
            .Where(p => !string.IsNullOrWhiteSpace(p.Title))
            .Select(p => new CvProjectDraft(
                Clamp(p.Title.Trim(), 200)!,
                Clamp(p.Description, 2000),
                Clamp(p.Image, 500),
                NormalizeUrl(p.LiveUrl),
                NormalizeUrl(p.GithubUrl),
                (p.Technologies ?? []).Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => Clamp(t.Trim(), 50)!).ToList(),
                p.IsFeatured))
            .ToList();

        // 5. Education sanitization
        var educations = (draft.Education ?? [])
            .Where(e => !string.IsNullOrWhiteSpace(e.InstitutionName) || !string.IsNullOrWhiteSpace(e.Degree))
            .Select(e => new CvEducationDraft(
                Clamp(e.InstitutionName.Trim(), 200) ?? "Institution",
                Clamp(e.Degree.Trim(), 150) ?? "Degree",
                Clamp(e.FieldOfStudy, 150),
                Clamp(e.Description, 1000),
                e.StartDate,
                e.IsCurrent ? null : e.EndDate,
                e.IsCurrent))
            .ToList();

        // 6. Certifications sanitization
        var certifications = (draft.Certifications ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .Select(c => new CvCertificationDraft(
                Clamp(c.Name.Trim(), 200)!,
                Clamp(c.IssuingOrganization?.Trim() ?? "Organization", 200)!,
                c.IssueDate,
                Clamp(c.CredentialId, 100),
                NormalizeUrl(c.CredentialUrl),
                Clamp(c.Description, 1000)))
            .ToList();

        // 7. Social Links sanitization
        var socialLinks = (draft.SocialLinks ?? [])
            .Where(s => !string.IsNullOrWhiteSpace(s.Platform) && !string.IsNullOrWhiteSpace(s.Url))
            .Select(s => new CvSocialLinkDraft(
                Clamp(s.Platform.Trim(), 50)!,
                NormalizeUrl(s.Url) ?? s.Url.Trim()))
            .ToList();

        return new CvPortfolioDraft(
            personalInfo,
            skills,
            experiences,
            projects,
            educations,
            certifications,
            socialLinks);
    }

    private static string? Clamp(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string? NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var trimmed = url.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return uriResult.ToString();
        }

        if (Uri.TryCreate($"https://{trimmed}", UriKind.Absolute, out var httpsUriResult) &&
            (httpsUriResult.Scheme == Uri.UriSchemeHttp || httpsUriResult.Scheme == Uri.UriSchemeHttps))
        {
            return httpsUriResult.ToString();
        }

        return null;
    }
}
