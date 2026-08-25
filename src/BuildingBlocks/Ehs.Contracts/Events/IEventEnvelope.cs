namespace Ehs.Contracts.Events;

/// <summary>
/// Non-generic view of an event envelope, for code that needs to inspect/log
/// envelope metadata (dispatch, outbox serialization) without depending on
/// the concrete payload type.
/// </summary>
public interface IEventEnvelope
{
    Guid EventId { get; }

    string EventType { get; }

    int EventVersion { get; }

    DateTime OccurredOnUtc { get; }

    Guid CorrelationId { get; }

    Guid? CausationId { get; }

    string Source { get; }

    Guid? TenantId { get; }

    string? UserId { get; }
}
