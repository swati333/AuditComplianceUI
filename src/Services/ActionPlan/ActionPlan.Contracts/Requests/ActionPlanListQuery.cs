namespace ActionPlan.Contracts.Requests;

/// <summary>
/// List-endpoint query shape per CLAUDE.md §6: pagination/sort/search plus
/// filters specific to action plans. Flat, like Finding's FindingListQuery,
/// so ASP.NET Core can bind it directly from a flat query string.
/// </summary>
public sealed class ActionPlanListQuery
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SortBy { get; set; }

    public string SortDirection { get; set; } = "asc";

    public string? SearchText { get; set; }

    public Guid? FindingId { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public string? OwnerId { get; set; }

    public string? ApproverId { get; set; }

    public DateTime? DueBefore { get; set; }

    public DateTime? DueAfter { get; set; }

    /// <summary>Convenience toggle equivalent to Status=Overdue, kept distinct so the UI can offer a one-click "overdue" filter alongside the general status dropdown.</summary>
    public bool? IsOverdue { get; set; }
}
