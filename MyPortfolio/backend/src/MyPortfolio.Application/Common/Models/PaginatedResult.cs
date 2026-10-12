namespace MyPortfolio.Application.Common.Models;

public class PaginatedResult<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedResult(IReadOnlyCollection<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PaginatedResult<T> Create(IReadOnlyCollection<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
