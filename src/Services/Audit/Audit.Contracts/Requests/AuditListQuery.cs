namespace Audit.Contracts.Requests;

/// <summary>
/// List-endpoint query shape per CLAUDE.md §6: pagination/sort/search plus
/// filters specific to audits. Flat (not composing the sealed
/// Ehs.SharedKernel <c>PagedRequest</c>) so ASP.NET Core can bind it directly
/// from a flat query string (<c>?pageNumber=1&amp;pageSize=20&amp;status=...</c>);
/// the Application layer maps it onto <c>PagedRequest</c> internally.
/// </summary>
public sealed class AuditListQuery
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SortBy { get; set; }

    public string SortDirection { get; set; } = "asc";

    public string? SearchText { get; set; }

    public string? Status { get; set; }

    public string? Location { get; set; }

    public DateTime? PlannedStartDateFrom { get; set; }

    public DateTime? PlannedStartDateTo { get; set; }
}
