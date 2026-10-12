using MyPortfolio.Domain.Common;

namespace MyPortfolio.Domain.Entities;

/// <summary>
/// Represents an incoming message sent through the portfolio contact form.
/// </summary>
public class ContactMessage : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    // Parameterless constructor for EF Core / Deserialization
    private ContactMessage()
    {
    }

    public ContactMessage(
        string name,
        string email,
        string subject,
        string message)
    {
        ValidateRequiredFields(name, email, subject, message);

        Name = name.Trim();
        Email = email.Trim();
        Subject = subject.Trim();
        Message = message.Trim();
        IsRead = false;
        ReadAt = null;
    }

    public void MarkAsRead()
    {
        if (IsRead && ReadAt.HasValue)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void MarkAsUnread()
    {
        if (!IsRead && !ReadAt.HasValue)
            return;

        IsRead = false;
        ReadAt = null;
        MarkUpdated();
    }

    private static void ValidateRequiredFields(string name, string email, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Sender name cannot be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Sender email cannot be null or empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Message subject cannot be null or empty.", nameof(subject));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message content cannot be null or empty.", nameof(message));
    }
}
