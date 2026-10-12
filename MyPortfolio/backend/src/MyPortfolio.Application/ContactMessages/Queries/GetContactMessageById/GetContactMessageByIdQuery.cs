using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.ContactMessages.Models;

namespace MyPortfolio.Application.ContactMessages.Queries.GetContactMessageById;

public record GetContactMessageByIdQuery(Guid Id) : IRequest<Result<ContactMessageDto>>;

public class GetContactMessageByIdQueryHandler : IRequestHandler<GetContactMessageByIdQuery, Result<ContactMessageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetContactMessageByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ContactMessageDto>> Handle(
        GetContactMessageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var message = await _context.ContactMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (message == null)
        {
            return Result<ContactMessageDto>.Failure($"Contact message with ID '{request.Id}' was not found.");
        }

        return Result<ContactMessageDto>.Success(ContactMessageDto.FromEntity(message));
    }
}
