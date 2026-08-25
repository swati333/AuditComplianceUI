using Ehs.Contracts.Events;
using Ehs.EventBus;
using Ehs.SharedKernel.Messaging;
using Finding.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finding.Infrastructure.Messaging;

/// <summary>
/// Consume-side of the Inbox pattern (CLAUDE.md §8): checks
/// <see cref="InboxMessage"/> before invoking a handler so a redelivered
/// event never repeats its side effect, then records the (eventId,
/// consumerName) pair once the handler completes. <paramref name="handler"/>
/// is injected per call rather than resolved by type, so a caller can supply
/// any <see cref="IIntegrationEventHandler{TPayload}"/> implementation
/// without this class needing to know the DI container's registrations.
///
/// There is no real Service Bus subscription driving this yet (Azure Service
/// Bus wiring is a later infra phase per CLAUDE.md's phase plan) — a future
/// background consumer would call <see cref="ConsumeAsync{TPayload}"/> once
/// per message it pulls off the wire. For now this is exercised directly by
/// integration tests, simulating "a message arrived."
/// </summary>
public sealed class IntegrationEventConsumer
{
    private readonly FindingDbContext _dbContext;

    public IntegrationEventConsumer(FindingDbContext dbContext)
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
