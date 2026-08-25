using ActionPlan.Infrastructure.Persistence;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Ehs.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ActionPlan.Infrastructure.Messaging;

/// <summary>
/// Consume-side of the Inbox pattern (CLAUDE.md §8): checks
/// <see cref="InboxMessage"/> before invoking a handler so a redelivered
/// event never repeats its side effect, then records the (eventId,
/// consumerName) pair once the handler completes. See Finding.Infrastructure's
/// identical consumer for the full rationale, including why there's no real
/// Service Bus subscription driving this yet.
/// </summary>
public sealed class IntegrationEventConsumer
{
    private readonly ActionPlanDbContext _dbContext;

    public IntegrationEventConsumer(ActionPlanDbContext dbContext)
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
