using Microsoft.EntityFrameworkCore;
using Notification.Application.Common;
using Notification.Contracts.Requests;
using Notification.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(NotificationEntity notification) => _dbContext.Notifications.Add(notification);

    public Task<NotificationEntity?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<NotificationEntity?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Notifications.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<NotificationEntity> Items, int TotalCount)> SearchAsync(NotificationListQuery query, CancellationToken cancellationToken = default)
    {
        var filtered = _dbContext.Notifications.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.RecipientUserId))
        {
            filtered = filtered.Where(n => n.RecipientUserId == query.RecipientUserId);
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<NotificationStatus>(query.Status, ignoreCase: true, out var status))
        {
            filtered = filtered.Where(n => n.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Channel) && Enum.TryParse<NotificationChannel>(query.Channel, ignoreCase: true, out var channel))
        {
            filtered = filtered.Where(n => n.Channel == channel);
        }

        var totalCount = await filtered.CountAsync(cancellationToken);

        var sorted = ApplySort(filtered, query.SortBy, query.SortDirection);

        var items = await sorted
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>Always appends a final <c>ThenBy(Id)</c> tie-breaker so paging is deterministic (CLAUDE.md §4).</summary>
    private static IOrderedQueryable<NotificationEntity> ApplySort(IQueryable<NotificationEntity> query, string? sortBy, string sortDirection)
    {
        var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<NotificationEntity> ordered = sortBy?.Trim().ToLowerInvariant() switch
        {
            "status" => descending ? query.OrderByDescending(n => n.Status) : query.OrderBy(n => n.Status),
            "channel" => descending ? query.OrderByDescending(n => n.Channel) : query.OrderBy(n => n.Channel),
            _ => descending ? query.OrderByDescending(n => n.CreatedDate) : query.OrderBy(n => n.CreatedDate),
        };

        return ordered.ThenBy(n => n.Id);
    }
}
