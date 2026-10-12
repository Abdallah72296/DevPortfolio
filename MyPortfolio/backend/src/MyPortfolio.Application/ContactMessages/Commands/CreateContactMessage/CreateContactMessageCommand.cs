using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.ContactMessages.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.ContactMessages.Commands.CreateContactMessage;

public record CreateContactMessageCommand(
    string Name,
    string Email,
    string Subject,
    string Message,
    string? Username = null) : IRequest<Result<ContactMessageDto>>;

public class CreateContactMessageCommandHandler : IRequestHandler<CreateContactMessageCommand, Result<ContactMessageDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateContactMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ContactMessageDto>> Handle(
        CreateContactMessageCommand request,
        CancellationToken cancellationToken)
    {
        var message = new ContactMessage(
            name: request.Name,
            email: request.Email,
            subject: request.Subject,
            message: request.Message);

        var profiles = _context.UserProfiles.AsQueryable();
        MyPortfolio.Domain.Entities.UserProfile? profile = null;
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var username = request.Username.Trim().ToLowerInvariant();
            profile = await profiles.IgnoreQueryFilters().FirstOrDefaultAsync(p =>
                p.Email.ToLower() == username || p.Email.ToLower().StartsWith(username + "@") ||
                p.FullName.ToLower() == username || p.UserId.ToLower() == username, cancellationToken);
        }
        profile ??= await profiles.OrderBy(p => p.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (profile != null) message.AssignOwner(profile.OwnerUserId);

        _context.ContactMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ContactMessageDto>.Success(
            ContactMessageDto.FromEntity(message),
            "Your message has been sent successfully.");
    }
}
