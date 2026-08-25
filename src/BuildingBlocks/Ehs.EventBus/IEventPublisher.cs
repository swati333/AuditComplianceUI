using Ehs.Contracts.Events;

namespace Ehs.EventBus;

/// <summary>
/// Publishes integration events. The Infrastructure implementation is
/// expected to write to the transactional outbox (CLAUDE.md §8) rather than
/// calling Service Bus directly from request-handling code.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TPayload>(TPayload payload, CancellationToken cancellationToken = default)
        where TPayload : IIntegrationEvent;

    Task PublishAsync<TPayload>(EventEnvelope<TPayload> envelope, CancellationToken cancellationToken = default)
        where TPayload : IIntegrationEvent;
}
