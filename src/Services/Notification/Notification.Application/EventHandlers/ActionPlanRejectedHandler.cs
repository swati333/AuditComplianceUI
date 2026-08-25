using Ehs.Contracts.Events;
using Ehs.EventBus;
using Notification.Application.Services;
using Notification.Contracts.ExternalEvents;

namespace Notification.Application.EventHandlers;

public sealed class ActionPlanRejectedHandler : IIntegrationEventHandler<ActionPlanRejected>
{
    private const string EventCode = "ActionPlanRejected";

    private readonly NotificationFactory _notificationFactory;

    public ActionPlanRejectedHandler(NotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }

    public Task HandleAsync(EventEnvelope<ActionPlanRejected> envelope, CancellationToken cancellationToken = default)
    {
        var placeholders = new Dictionary<string, string>
        {
            ["ActionPlanId"] = envelope.Payload.ActionPlanId.ToString(),
            ["FindingId"] = envelope.Payload.FindingId.ToString(),
            ["RejectedByUserId"] = envelope.Payload.RejectedByUserId,
            ["Reason"] = envelope.Payload.Reason,
        };

        return _notificationFactory.CreateFromEventAsync(EventCode, envelope.Payload.AssignedToUserId, placeholders, envelope.CorrelationId, cancellationToken);
    }
}
