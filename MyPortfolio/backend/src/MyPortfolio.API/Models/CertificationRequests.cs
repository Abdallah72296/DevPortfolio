using MyPortfolio.Application.Certifications.Commands.ReorderCertifications;

namespace MyPortfolio.API.Models;

public record CreateCertificationRequest(
    string Name,
    string IssuingOrganization,
    DateTime IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description,
    int DisplayOrder);

public record UpdateCertificationRequest(
    string Name,
    string IssuingOrganization,
    DateTime IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description,
    int DisplayOrder);

public record SetCertificationVisibilityRequest(bool IsVisible);

public record ReorderCertificationsRequest(List<CertificationOrderItem> Items);
