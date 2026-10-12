using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Certifications.Models;

public record CertificationDto(
    Guid Id,
    string Name,
    string IssuingOrganization,
    DateTime IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description,
    int DisplayOrder,
    bool IsVisible)
{
    public static CertificationDto FromEntity(Certification cert) =>
        new(
            cert.Id,
            cert.Name,
            cert.IssuingOrganization,
            cert.IssueDate,
            cert.CredentialId,
            cert.CredentialUrl,
            cert.Description,
            cert.DisplayOrder,
            cert.IsVisible);
}
