using Ehs.Contracts.Events;
using Ehs.EventBus;
using Notification.Application.Services;
using Notification.Contracts.ExternalEvents;

namespace Notification.Application.EventHandlers;

/// <summary>Confirms receipt to the submitter — the real "please review" notification to an approver needs a resolvable approver identity that Action Plan Service doesn't provide yet (it doesn't exist in this phase).</summary>
public sealed class ActionPlanSubmittedHandler : IIntegrationEventHandler<ActionPlanSubmitted>
{
    private const string EventCode = "ActionPlanSubmitted";

    private readonly NotificationFactory _notificationFactory;

    public ActionPlanSubmittedHandler(NotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }

    public Task HandleAsync(EventEnvelope<ActionPlanSubmitted> envelope, CancellationToken cancellationToken = default)
    {
        var placeholders = new Dictionary<string, string>
        {
            ["ActionPlanId"] = envelope.Payload.ActionPlanId.ToString(),
            ["FindingId"] = envelope.Payload.FindingId.ToString(),
        };

        return _notificationFactory.CreateFromEventAsync(EventCode, envelope.Payload.SubmittedByUserId, placeholders, envelope.CorrelationId, cancellationToken);
    }
}
