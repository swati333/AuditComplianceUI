using Ehs.Contracts.Events;
using Ehs.EventBus;
using Ehs.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Infrastructure.Persistence;
using Serilog.Context;

namespace Notification.Infrastructure.Messaging;

/// <summary>
/// Consume-side of the Inbox pattern (CLAUDE.md §8). Two things beyond
/// Finding Service's identical-looking consumer:
///
/// 1. Atomicity: the handler must only stage changes (repository.Add, no
///    SaveChanges of its own) — this class calls SaveChangesAsync exactly
///    once, after adding the Inbox row, so the notification(s) a handler
///    creates and the Inbox row that marks the event processed commit
///    together or not at all. Calling SaveChanges twice (once inside the
///    handler, once here) would leave a crash window where the handler's
///    effect is durable but the event isn't marked processed — a
///    redelivery would then repeat the effect, which is exactly what Inbox
///    exists to prevent.
/// 2. Correlation: pushes the envelope's CorrelationId onto Serilog's
///    LogContext for the duration of handling, so every log line emitted
///    while processing one event — including from the handler and from
///    NotificationFactory — carries the same CorrelationId a reader would
///    see on the originating HTTP request in the publishing service
///    (CLAUDE.md §7: "Correlation and causation IDs propagate through
///    requests, logs and events end-to-end").
///
/// No real Service Bus subscription drives this yet, same as Finding
/// Service — exercised directly by tests, simulating "a message arrived."
/// </summary>
public sealed class IntegrationEventConsumer
{
    private readonly NotificationDbContext _dbContext;
    private readonly ILogger<IntegrationEventConsumer> _logger;

    public IntegrationEventConsumer(NotificationDbContext dbContext, ILogger<IntegrationEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <returns>true if the handler ran (first delivery); false if this (eventId, consumerName) pair was already processed.</returns>
    public async Task<bool> ConsumeAsync<TPayload>(
        EventEnvelope<TPayload> envelope,
        string consumerName,
        IIntegrationEventHandler<TPayload> handler,
        CancellationToken cancellationToken = default)
        where TPayload : IIntegrationEvent
    {
        using (LogContext.PushProperty("CorrelationId", envelope.CorrelationId))
        using (LogContext.PushProperty("SourceEventType", envelope.EventType))
        {
            var alreadyProcessed = await _dbContext.InboxMessages
                .AnyAsync(m => m.Id == envelope.EventId && m.ConsumerName == consumerName, cancellationToken);
            if (alreadyProcessed)
            {
                _logger.LogDebug("Skipping already-processed event {EventId} for consumer {ConsumerName}.", envelope.EventId, consumerName);
                return false;
            }

            await handler.HandleAsync(envelope, cancellationToken);

            _dbContext.InboxMessages.Add(new InboxMessage
            {
                Id = envelope.EventId,
                ConsumerName = consumerName,
                Type = envelope.EventType,
                ProcessedOnUtc = DateTime.UtcNow,
            });
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Processed event {EventId} ({EventType}) for consumer {ConsumerName}.", envelope.EventId, envelope.EventType, consumerName);
            return true;
        }
    }
}
