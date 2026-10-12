using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.ContactMessages.Models;

namespace MyPortfolio.Application.ContactMessages.Queries.GetContactMessages;

public record GetContactMessagesQuery(
    int Page = 1,
    int PageSize = 10,
    bool? IsRead = null,
    string? Search = null) : IRequest<Result<PaginatedResult<ContactMessageListItemDto>>>;

public class GetContactMessagesQueryHandler : IRequestHandler<GetContactMessagesQuery, Result<PaginatedResult<ContactMessageListItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetContactMessagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<ContactMessageListItemDto>>> Handle(
        GetContactMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.ContactMessages
            .AsNoTracking()
            .AsQueryable();

        if (request.IsRead.HasValue)
        {
            query = query.Where(m => m.IsRead == request.IsRead.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(m =>
                m.Name.Contains(search) ||
                m.Email.Contains(search) ||
                m.Subject.Contains(search) ||
                m.Message.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new ContactMessageListItemDto(
                m.Id,
                m.Name,
                m.Email,
                m.Subject,
                m.Message,
                m.IsRead,
                m.ReadAt,
                m.CreatedAt))
            .ToListAsync(cancellationToken);

        var paginatedResult = PaginatedResult<ContactMessageListItemDto>.Create(
            items,
            totalCount,
            request.Page,
            request.PageSize);

        return Result<PaginatedResult<ContactMessageListItemDto>>.Success(paginatedResult);
    }
}
