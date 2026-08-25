using Notification.Domain.Entities;
using Notification.Domain.Enums;

namespace Notification.Application.Common;

public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetAsync(string userId, NotificationChannel channel, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationPreference>> ListForUserAsync(string userId, CancellationToken cancellationToken = default);

    void Add(NotificationPreference preference);
}
