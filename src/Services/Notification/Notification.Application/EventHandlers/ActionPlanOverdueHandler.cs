using Ehs.Contracts.Events;
using Ehs.EventBus;
using Notification.Application.Services;
using Notification.Contracts.ExternalEvents;

namespace Notification.Application.EventHandlers;

public sealed class ActionPlanOverdueHandler : IIntegrationEventHandler<ActionPlanOverdue>
{
    private const string EventCode = "ActionPlanOverdue";

    private readonly NotificationFactory _notificationFactory;

    public ActionPlanOverdueHandler(NotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }

    public Task HandleAsync(EventEnvelope<ActionPlanOverdue> envelope, CancellationToken cancellationToken = default)
    {
        var placeholders = new Dictionary<string, string>
        {
            ["ActionPlanId"] = envelope.Payload.ActionPlanId.ToString(),
            ["FindingId"] = envelope.Payload.FindingId.ToString(),
            ["DueDate"] = envelope.Payload.DueDate.ToString("yyyy-MM-dd"),
        };

        return _notificationFactory.CreateFromEventAsync(EventCode, envelope.Payload.AssignedToUserId, placeholders, envelope.CorrelationId, cancellationToken);
    }
}
