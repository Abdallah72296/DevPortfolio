using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.ContactMessages.Models;

namespace MyPortfolio.Application.ContactMessages.Commands.MarkContactMessageAsRead;

public record MarkContactMessageAsReadCommand(Guid Id) : IRequest<Result<ContactMessageDto>>;

public class MarkContactMessageAsReadCommandHandler : IRequestHandler<MarkContactMessageAsReadCommand, Result<ContactMessageDto>>
{
    private readonly IApplicationDbContext _context;

    public MarkContactMessageAsReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ContactMessageDto>> Handle(
        MarkContactMessageAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var message = await _context.ContactMessages
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (message == null)
        {
            return Result<ContactMessageDto>.Failure($"Contact message with ID '{request.Id}' was not found.");
        }

        message.MarkAsRead();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ContactMessageDto>.Success(
            ContactMessageDto.FromEntity(message),
            "Contact message marked as read.");
    }
}
