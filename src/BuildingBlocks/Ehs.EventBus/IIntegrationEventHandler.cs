using Ehs.Contracts.Events;

namespace Ehs.EventBus;

/// <summary>
/// Consumer of a specific integration event. Implementations are expected to
/// check the Inbox (CLAUDE.md §8) before applying the side effect, so that a
/// redelivered message never duplicates a business operation.
/// </summary>
public interface IIntegrationEventHandler<TPayload>
    where TPayload : IIntegrationEvent
{
    Task HandleAsync(EventEnvelope<TPayload> envelope, CancellationToken cancellationToken = default);
}
