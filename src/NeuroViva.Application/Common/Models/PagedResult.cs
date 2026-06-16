namespace NeuroViva.Application.Common.Models;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public sealed record PaginationParams
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip => (Page - 1) * PageSize;

    public PaginationParams WithMaxPageSize(int max) =>
        PageSize > max ? this with { PageSize = max } : this;
}
