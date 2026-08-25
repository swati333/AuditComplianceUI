namespace Notification.Contracts.Requests;

/// <summary>List-endpoint query shape per CLAUDE.md §6. Flat, like the other services' list queries, so ASP.NET Core can bind it directly from a flat query string.</summary>
public sealed class NotificationListQuery
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SortBy { get; set; }

    public string SortDirection { get; set; } = "desc";

    public string? RecipientUserId { get; set; }

    public string? Status { get; set; }

    public string? Channel { get; set; }
}
