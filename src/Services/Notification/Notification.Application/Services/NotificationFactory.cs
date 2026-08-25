using Notification.Application.Common;
using Notification.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.Services;

/// <summary>
/// Shared "resolve templates → check preferences → render → create" logic
/// used by every integration-event handler, so each of the 8 handlers stays
/// a thin adapter from its specific event payload to a placeholder map.
/// Does not call SaveChanges — the caller (IntegrationEventConsumer) commits
/// once, atomically with the Inbox row, after the handler returns.
/// </summary>
public sealed class NotificationFactory
{
    private const string SystemActor = "system";

    private readonly INotificationTemplateRepository _templateRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly INotificationRepository _notificationRepository;

    public NotificationFactory(
        INotificationTemplateRepository templateRepository,
        INotificationPreferenceRepository preferenceRepository,
        INotificationRepository notificationRepository)
    {
        _templateRepository = templateRepository;
        _preferenceRepository = preferenceRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task CreateFromEventAsync(
        string eventCode,
        string? recipientUserId,
        IReadOnlyDictionary<string, string> placeholders,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        var templates = await _templateRepository.GetActiveByCodeAsync(eventCode, cancellationToken);

        foreach (var template in templates)
        {
            // No resolvable recipient (broadcast) → nothing to address an email to.
            if (recipientUserId is null && template.Channel == NotificationChannel.Email)
            {
                continue;
            }

            if (recipientUserId is not null)
            {
                var preference = await _preferenceRepository.GetAsync(recipientUserId, template.Channel, cancellationToken);
                if (preference is { IsEnabled: false })
                {
                    continue;
                }
            }

            var (subject, body) = template.Render(placeholders);
            var notification = NotificationEntity.Create(
                recipientUserId,
                template.Channel,
                template.Code,
                subject,
                body,
                correlationId,
                eventCode,
                SystemActor);

            _notificationRepository.Add(notification);
        }
    }
}
