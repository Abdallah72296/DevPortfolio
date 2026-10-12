using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>
/// Represents the personal profile and hero information of a portfolio owner, tied to an Identity user.
/// </summary>
public class UserProfile : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string HeadlineRole { get; private set; } = string.Empty;
    public string Badge { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string CardCaption { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string LongBio { get; private set; } = string.Empty;
    public string AvatarUrl { get; private set; } = string.Empty;
    public string CvUrl { get; private set; } = string.Empty;
    public string YearsOfExperience { get; private set; } = string.Empty;
    public string ParagraphsJson { get; private set; } = "[]";
    public bool AvailableForHire { get; private set; } = true;

    // EF Core constructor
    private UserProfile()
    {
    }

    public UserProfile(
        string userId,
        string fullName,
        string title,
        string headlineRole,
        string badge,
        string location,
        string email,
        string phone,
        string summary,
        string longBio,
        string avatarUrl,
        string cvUrl,
        string yearsOfExperience,
        string paragraphsJson,
        bool availableForHire = true,
        string cardCaption = "")
    {
        UserId = userId;
        FullName = fullName;
        Title = title;
        HeadlineRole = headlineRole;
        Badge = badge;
        Location = location;
        Email = email;
        Phone = phone;
        Summary = summary;
        LongBio = longBio;
        AvatarUrl = avatarUrl;
        CvUrl = cvUrl;
        YearsOfExperience = yearsOfExperience;
        ParagraphsJson = string.IsNullOrWhiteSpace(paragraphsJson) ? "[]" : paragraphsJson;
        AvailableForHire = availableForHire;
        CardCaption = string.IsNullOrWhiteSpace(cardCaption) ? $"{title} • {location}" : cardCaption;
    }

    public void UpdateProfile(
        string fullName,
        string title,
        string headlineRole,
        string badge,
        string location,
        string email,
        string phone,
        string summary,
        string longBio,
        string avatarUrl,
        string cvUrl,
        string yearsOfExperience,
        string paragraphsJson,
        bool availableForHire,
        string cardCaption)
    {
        FullName = fullName;
        Title = title;
        HeadlineRole = headlineRole;
        Badge = badge;
        Location = location;
        Email = email;
        Phone = phone;
        Summary = summary;
        LongBio = longBio;
        AvatarUrl = avatarUrl;
        CvUrl = cvUrl;
        YearsOfExperience = yearsOfExperience;
        ParagraphsJson = string.IsNullOrWhiteSpace(paragraphsJson) ? "[]" : paragraphsJson;
        AvailableForHire = availableForHire;
        CardCaption = string.IsNullOrWhiteSpace(cardCaption) ? $"{title} • {location}" : cardCaption;
        MarkUpdated();
    }

    public void SetAvatarUrl(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        MarkUpdated();
    }

    public void AssignToUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID is required.", nameof(userId));
        UserId = userId;
        AssignOwner(userId);
    }

    public void SetCvUrl(string cvUrl)
    {
        CvUrl = cvUrl?.Trim() ?? string.Empty;
        MarkUpdated();
    }
}
