namespace Finding.Contracts.Requests;

/// <summary>
/// List-endpoint query shape per CLAUDE.md §6: pagination/sort/search plus
/// filters specific to findings. Flat, like Audit's AuditListQuery, so
/// ASP.NET Core can bind it directly from a flat query string.
/// </summary>
public sealed class FindingListQuery
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SortBy { get; set; }

    public string SortDirection { get; set; } = "asc";

    public string? SearchText { get; set; }

    public Guid? AuditId { get; set; }

    public string? Status { get; set; }

    public string? Severity { get; set; }
}
