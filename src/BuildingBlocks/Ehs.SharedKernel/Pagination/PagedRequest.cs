namespace Ehs.SharedKernel.Pagination;

/// <summary>
/// Common list-endpoint query shape: pagination, sorting and free-text search,
/// per the REST conventions in CLAUDE.md §6. Entity-specific filters are added
/// by each service's own request DTO (composition, not inheritance).
/// </summary>
public sealed class PagedRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 1,
            > MaxPageSize => MaxPageSize,
            _ => value,
        };
    }

    public string? SortBy { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    public string? SearchText { get; set; }
}
