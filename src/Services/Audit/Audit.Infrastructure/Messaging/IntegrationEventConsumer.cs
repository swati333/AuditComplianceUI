using Audit.Infrastructure.Persistence;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Ehs.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Messaging;

/// <summary>
/// Consume-side of the Inbox pattern (CLAUDE.md §8) — identical shape to
/// Finding.Infrastructure.Messaging.IntegrationEventConsumer. Audit Service
/// only publishes until now; this is its first consumer, added to back
/// IAuditComplianceGateway with a real read model synced from Finding and
/// Action Plan Service events. As with Finding's consumer, there is no live
/// Service Bus subscription driving this yet — a future background consumer
/// would call <see cref="ConsumeAsync{TPayload}"/> once per message it pulls
/// off the wire; for now this is exercised directly by integration tests.
/// </summary>
public sealed class IntegrationEventConsumer
{
    private readonly AuditDbContext _dbContext;

    public IntegrationEventConsumer(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <returns>true if the handler ran (first delivery); false if this (eventId, consumerName) pair was already processed.</returns>
    public async Task<bool> ConsumeAsync<TPayload>(
        EventEnvelope<TPayload> envelope,
        string consumerName,
        IIntegrationEventHandler<TPayload> handler,
        CancellationToken cancellationToken = default)
        where TPayload : IIntegrationEvent
    {
        var alreadyProcessed = await _dbContext.InboxMessages
            .AnyAsync(m => m.Id == envelope.EventId && m.ConsumerName == consumerName, cancellationToken);
        if (alreadyProcessed)
        {
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

        return true;
    }
}
