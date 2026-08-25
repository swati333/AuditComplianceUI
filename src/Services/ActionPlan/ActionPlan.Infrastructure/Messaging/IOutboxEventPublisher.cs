using Ehs.SharedKernel.Messaging;

namespace ActionPlan.Infrastructure.Messaging;

/// <summary>Hands one already-serialized outbox row to the transport (service-local, not Ehs.EventBus's generic IEventPublisher — see Finding.Infrastructure for the same rationale).</summary>
public interface IOutboxEventPublisher
{
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
