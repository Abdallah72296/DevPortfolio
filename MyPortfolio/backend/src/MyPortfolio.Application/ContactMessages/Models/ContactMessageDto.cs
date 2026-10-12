using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.ContactMessages.Models;

public record ContactMessageDto(
    Guid Id,
    string Name,
    string Email,
    string Subject,
    string Message,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static ContactMessageDto FromEntity(ContactMessage message) =>
        new(
            message.Id,
            message.Name,
            message.Email,
            message.Subject,
            message.Message,
            message.IsRead,
            message.ReadAt,
            message.CreatedAt,
            message.UpdatedAt);
}
