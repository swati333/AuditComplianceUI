using Notification.Contracts.Requests;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.Common;

public interface INotificationRepository
{
    Task<NotificationEntity?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<NotificationEntity?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<NotificationEntity> Items, int TotalCount)> SearchAsync(NotificationListQuery query, CancellationToken cancellationToken = default);

    void Add(NotificationEntity notification);
}
