using Ehs.Contracts.Events;
using Ehs.EventBus;
using Notification.Application.Services;
using Notification.Contracts.ExternalEvents;

namespace Notification.Application.EventHandlers;

public sealed class ReportGeneratedHandler : IIntegrationEventHandler<ReportGenerated>
{
    private const string EventCode = "ReportGenerated";

    private readonly NotificationFactory _notificationFactory;

    public ReportGeneratedHandler(NotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }

    public Task HandleAsync(EventEnvelope<ReportGenerated> envelope, CancellationToken cancellationToken = default)
    {
        var placeholders = new Dictionary<string, string>
        {
            ["ReportId"] = envelope.Payload.ReportId.ToString(),
            ["ReportType"] = envelope.Payload.ReportType,
        };

        return _notificationFactory.CreateFromEventAsync(EventCode, envelope.Payload.RequestedByUserId, placeholders, envelope.CorrelationId, cancellationToken);
    }
}
