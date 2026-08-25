using Ehs.SharedKernel.Messaging;

namespace Audit.Infrastructure.Messaging;

/// <summary>
/// Hands one already-serialized outbox row to the transport. Deliberately
/// service-local (not Ehs.EventBus's IEventPublisher, which is generic
/// over a strongly-typed payload) because the dispatcher works off the raw,
/// already-persisted envelope JSON, not a live .NET object.
/// </summary>
public interface IOutboxEventPublisher
{
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
