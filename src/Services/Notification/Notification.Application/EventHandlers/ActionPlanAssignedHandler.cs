using Ehs.Contracts.Events;
using Ehs.EventBus;
using Notification.Application.Services;
using Notification.Contracts.ExternalEvents;

namespace Notification.Application.EventHandlers;

public sealed class ActionPlanAssignedHandler : IIntegrationEventHandler<ActionPlanAssigned>
{
    private const string EventCode = "ActionPlanAssigned";

    private readonly NotificationFactory _notificationFactory;

    public ActionPlanAssignedHandler(NotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }

    public Task HandleAsync(EventEnvelope<ActionPlanAssigned> envelope, CancellationToken cancellationToken = default)
    {
        var placeholders = new Dictionary<string, string>
        {
            ["ActionPlanId"] = envelope.Payload.ActionPlanId.ToString(),
            ["FindingId"] = envelope.Payload.FindingId.ToString(),
            ["Title"] = envelope.Payload.Title,
            ["DueDate"] = envelope.Payload.DueDate.ToString("yyyy-MM-dd"),
        };

        return _notificationFactory.CreateFromEventAsync(EventCode, envelope.Payload.AssignedToUserId, placeholders, envelope.CorrelationId, cancellationToken);
    }
}
