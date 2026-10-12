using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.ContactMessages.Models;

public record ContactMessageListItemDto(
    Guid Id,
    string Name,
    string Email,
    string Subject,
    string Message,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt)
{
    public static ContactMessageListItemDto FromEntity(ContactMessage message) =>
        new(
            message.Id,
            message.Name,
            message.Email,
            message.Subject,
            message.Message,
            message.IsRead,
            message.ReadAt,
            message.CreatedAt);
}
