using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>
/// Represents a professional, teaching, or freelance work experience item in the Portfolio.
/// </summary>
public class Experience : BaseEntity
{
    public string JobTitle { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsCurrent { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }

    // Parameterless constructor for EF Core / Deserialization
    private Experience()
    {
    }

    public Experience(
        string jobTitle,
        string companyName,
        string description,
        DateTime startDate,
        DateTime? endDate,
        bool isCurrent,
        int displayOrder = 0,
        bool isVisible = true)
    {
        ValidateRequiredFields(jobTitle, companyName, description);
        ValidatePeriod(startDate, endDate, isCurrent);

        JobTitle = jobTitle.Trim();
        CompanyName = companyName.Trim();
        Description = description.Trim();
        StartDate = startDate;
        EndDate = isCurrent ? null : endDate;
        IsCurrent = isCurrent;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }

    public void UpdateDetails(string jobTitle, string companyName, string description)
    {
        ValidateRequiredFields(jobTitle, companyName, description);

        JobTitle = jobTitle.Trim();
        CompanyName = companyName.Trim();
        Description = description.Trim();
        MarkUpdated();
    }

    public void UpdatePeriod(DateTime startDate, DateTime? endDate, bool isCurrent)
    {
        ValidatePeriod(startDate, endDate, isCurrent);

        StartDate = startDate;
        EndDate = isCurrent ? null : endDate;
        IsCurrent = isCurrent;
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

    private static void ValidateRequiredFields(string jobTitle, string companyName, string description)
    {
        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new ArgumentException("Job title cannot be null or empty.", nameof(jobTitle));

        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be null or empty.", nameof(companyName));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));
    }

    private static void ValidatePeriod(DateTime startDate, DateTime? endDate, bool isCurrent)
    {
        if (isCurrent && endDate.HasValue)
            throw new InvalidOperationException("A current experience cannot have an end date.");

        if (!isCurrent && endDate.HasValue && endDate.Value < startDate)
            throw new ArgumentException("End date cannot precede the start date.", nameof(endDate));
    }
}
