using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>
/// Represents a professional certification or credential issued by an organization.
/// </summary>
public class Certification : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string IssuingOrganization { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }
    public string? CredentialId { get; private set; }
    public string? CredentialUrl { get; private set; }
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }

    // Parameterless constructor for EF Core / Deserialization
    private Certification()
    {
    }

    public Certification(
        string name,
        string issuingOrganization,
        DateTime issueDate,
        string? credentialId = null,
        string? credentialUrl = null,
        string? description = null,
        int displayOrder = 0,
        bool isVisible = true)
    {
        ValidateRequiredFields(name, issuingOrganization);

        Name = name.Trim();
        IssuingOrganization = issuingOrganization.Trim();
        IssueDate = issueDate;
        CredentialId = credentialId?.Trim();
        CredentialUrl = credentialUrl?.Trim();
        Description = description?.Trim();
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }

    public void UpdateDetails(
        string name,
        string issuingOrganization,
        DateTime issueDate,
        string? description)
    {
        ValidateRequiredFields(name, issuingOrganization);

        Name = name.Trim();
        IssuingOrganization = issuingOrganization.Trim();
        IssueDate = issueDate;
        Description = description?.Trim();
        MarkUpdated();
    }

    public void UpdateCredential(string? credentialId, string? credentialUrl)
    {
        CredentialId = credentialId?.Trim();
        CredentialUrl = credentialUrl?.Trim();
        MarkUpdated();
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        MarkUpdated();
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;
        MarkUpdated();
    }

    private static void ValidateRequiredFields(string name, string issuingOrganization)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Certification name cannot be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(issuingOrganization))
            throw new ArgumentException("Issuing organization cannot be null or empty.", nameof(issuingOrganization));
    }
}
