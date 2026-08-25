using Notification.Domain.Entities;

namespace Notification.Application.Common;

public interface INotificationTemplateRepository
{
    /// <summary>All active templates (one per channel) for the given event code — used by NotificationFactory to fan out a single event into per-channel notifications.</summary>
    Task<IReadOnlyList<NotificationTemplate>> GetActiveByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationTemplate>> ListAsync(CancellationToken cancellationToken = default);

    void Add(NotificationTemplate template);
}
