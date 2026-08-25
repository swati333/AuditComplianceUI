using Audit.Contracts.Events;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Notification.Application.Services;

namespace Notification.Application.EventHandlers;

/// <summary>
/// AuditPlanned carries no recipient (Audit.Contracts.Events.AuditPlanned is
/// just AuditId/dates) — creates a broadcast (InApp-only) notification. A
/// future phase could enrich this once Audit Service's event payload (or a
/// local team-membership projection) carries the assigned auditors/auditees.
/// </summary>
public sealed class AuditPlannedHandler : IIntegrationEventHandler<AuditPlanned>
{
    private const string EventCode = "AuditPlanned";

    private readonly NotificationFactory _notificationFactory;

    public AuditPlannedHandler(NotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }

    public Task HandleAsync(EventEnvelope<AuditPlanned> envelope, CancellationToken cancellationToken = default)
    {
        var placeholders = new Dictionary<string, string>
        {
            ["AuditId"] = envelope.Payload.AuditId.ToString(),
            ["PlannedStartDate"] = envelope.Payload.PlannedStartDate.ToString("yyyy-MM-dd"),
            ["PlannedEndDate"] = envelope.Payload.PlannedEndDate.ToString("yyyy-MM-dd"),
        };

        return _notificationFactory.CreateFromEventAsync(EventCode, recipientUserId: null, placeholders, envelope.CorrelationId, cancellationToken);
    }
}
