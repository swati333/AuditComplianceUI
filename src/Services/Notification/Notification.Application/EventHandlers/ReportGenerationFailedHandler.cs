using Ehs.Contracts.Events;
using Ehs.EventBus;
using Notification.Application.Services;
using Notification.Contracts.ExternalEvents;

namespace Notification.Application.EventHandlers;

public sealed class ReportGenerationFailedHandler : IIntegrationEventHandler<ReportGenerationFailed>
{
    private const string EventCode = "ReportGenerationFailed";

    private readonly NotificationFactory _notificationFactory;

    public ReportGenerationFailedHandler(NotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }

    public Task HandleAsync(EventEnvelope<ReportGenerationFailed> envelope, CancellationToken cancellationToken = default)
    {
        var placeholders = new Dictionary<string, string>
        {
            ["ReportId"] = envelope.Payload.ReportId.ToString(),
            ["ReportType"] = envelope.Payload.ReportType,
            ["ErrorMessage"] = envelope.Payload.ErrorMessage,
        };

        return _notificationFactory.CreateFromEventAsync(EventCode, envelope.Payload.RequestedByUserId, placeholders, envelope.CorrelationId, cancellationToken);
    }
}
