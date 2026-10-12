using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.ContactMessages.Commands.DeleteContactMessage;

public record DeleteContactMessageCommand(Guid Id) : IRequest<Result>;

public class DeleteContactMessageCommandHandler : IRequestHandler<DeleteContactMessageCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteContactMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteContactMessageCommand request,
        CancellationToken cancellationToken)
    {
        var message = await _context.ContactMessages
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (message == null)
        {
            return Result.Failure($"Contact message with ID '{request.Id}' was not found.");
        }

        _context.ContactMessages.Remove(message);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Contact message deleted successfully.");
    }
}
