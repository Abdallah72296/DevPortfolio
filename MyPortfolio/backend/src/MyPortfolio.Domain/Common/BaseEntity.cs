namespace MyPortfolio.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public string OwnerUserId { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    protected BaseEntity()
    {
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    public void MarkUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignOwner(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("Owner user ID is required.", nameof(userId));
        OwnerUserId = userId;
    }
}
