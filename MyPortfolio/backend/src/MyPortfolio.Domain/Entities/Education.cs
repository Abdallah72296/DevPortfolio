using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>
/// Represents an educational degree, institution, or academic background item.
/// </summary>
public class Education : BaseEntity
{
    public string InstitutionName { get; private set; } = string.Empty;
    public string Degree { get; private set; } = string.Empty;
    public string? FieldOfStudy { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsCurrent { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }

    // Parameterless constructor for EF Core / Deserialization
    private Education()
    {
    }

    public Education(
        string institutionName,
        string degree,
        DateTime startDate,
        DateTime? endDate,
        bool isCurrent,
        string? fieldOfStudy = null,
        string? description = null,
        int displayOrder = 0,
        bool isVisible = true)
    {
        ValidateRequiredFields(institutionName, degree);
        ValidatePeriod(startDate, endDate, isCurrent);

        InstitutionName = institutionName.Trim();
        Degree = degree.Trim();
        FieldOfStudy = fieldOfStudy?.Trim();
        Description = description?.Trim();
        StartDate = startDate;
        EndDate = isCurrent ? null : endDate;
        IsCurrent = isCurrent;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }

    public void UpdateDetails(
        string institutionName,
        string degree,
        string? fieldOfStudy,
        string? description)
    {
        ValidateRequiredFields(institutionName, degree);

        InstitutionName = institutionName.Trim();
        Degree = degree.Trim();
        FieldOfStudy = fieldOfStudy?.Trim();
        Description = description?.Trim();
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

    private static void ValidateRequiredFields(string institutionName, string degree)
    {
        if (string.IsNullOrWhiteSpace(institutionName))
            throw new ArgumentException("Institution name cannot be null or empty.", nameof(institutionName));

        if (string.IsNullOrWhiteSpace(degree))
            throw new ArgumentException("Degree cannot be null or empty.", nameof(degree));
    }

    private static void ValidatePeriod(DateTime startDate, DateTime? endDate, bool isCurrent)
    {
        if (isCurrent && endDate.HasValue)
            throw new InvalidOperationException("A current education cannot have an end date.");

        if (!isCurrent && endDate.HasValue && endDate.Value < startDate)
            throw new ArgumentException("End date cannot precede the start date.", nameof(endDate));
    }
}
