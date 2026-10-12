using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.ContactMessages.Models;

namespace MyPortfolio.Application.ContactMessages.Commands.MarkContactMessageAsUnread;

public record MarkContactMessageAsUnreadCommand(Guid Id) : IRequest<Result<ContactMessageDto>>;

public class MarkContactMessageAsUnreadCommandHandler : IRequestHandler<MarkContactMessageAsUnreadCommand, Result<ContactMessageDto>>
{
    private readonly IApplicationDbContext _context;

    public MarkContactMessageAsUnreadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ContactMessageDto>> Handle(
        MarkContactMessageAsUnreadCommand request,
        CancellationToken cancellationToken)
    {
        var message = await _context.ContactMessages
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (message == null)
        {
            return Result<ContactMessageDto>.Failure($"Contact message with ID '{request.Id}' was not found.");
        }

        message.MarkAsUnread();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ContactMessageDto>.Success(
            ContactMessageDto.FromEntity(message),
            "Contact message marked as unread.");
    }
}
