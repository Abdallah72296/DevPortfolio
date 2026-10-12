namespace MyPortfolio.Application.CV.Models;

public record CvCertificationDraft(
    string Name,
    string IssuingOrganization,
    string? IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description);
