namespace MyPortfolio.Application.Portfolio.Models;

public record PublicCertificationDto(
    Guid Id,
    string Name,
    string IssuingOrganization,
    DateTime IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description,
    int DisplayOrder);
