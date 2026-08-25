using Ehs.Contracts.Events;
using Ehs.EventBus;
using Finding.Contracts.Events;
using Notification.Application.Services;

namespace Notification.Application.EventHandlers;

/// <summary>Same broadcast rationale as AuditPlannedHandler — CriticalFindingCreated carries no specific recipient either.</summary>
public sealed class CriticalFindingCreatedHandler : IIntegrationEventHandler<CriticalFindingCreated>
{
    private const string EventCode = "CriticalFindingCreated";

    private readonly NotificationFactory _notificationFactory;

    public CriticalFindingCreatedHandler(NotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }

    public Task HandleAsync(EventEnvelope<CriticalFindingCreated> envelope, CancellationToken cancellationToken = default)
    {
        var placeholders = new Dictionary<string, string>
        {
            ["FindingId"] = envelope.Payload.FindingId.ToString(),
            ["AuditId"] = envelope.Payload.AuditId.ToString(),
            ["Title"] = envelope.Payload.Title,
        };

        return _notificationFactory.CreateFromEventAsync(EventCode, recipientUserId: null, placeholders, envelope.CorrelationId, cancellationToken);
    }
}
