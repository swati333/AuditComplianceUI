namespace Ehs.Contracts.Events;

/// <summary>
/// Mandatory envelope shape for every integration event published to Azure
/// Service Bus, per CLAUDE.md §7. <typeparamref name="TPayload"/> carries the
/// event-specific data; envelope fields carry cross-cutting metadata that is
/// identical for every event type.
/// </summary>
public sealed class EventEnvelope<TPayload> : IEventEnvelope
    where TPayload : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public required string EventType { get; init; }

    public int EventVersion { get; init; } = 1;

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    public required Guid CorrelationId { get; init; }

    public Guid? CausationId { get; init; }

    public required string Source { get; init; }

    public Guid? TenantId { get; init; }

    public string? UserId { get; init; }

    public required TPayload Payload { get; init; }
}

/// <summary>
/// Convenience factory so callers don't have to restate <c>EventType</c> and
/// <c>EventVersion</c> boilerplate on every publish call.
/// </summary>
public static class EventEnvelope
{
    public static EventEnvelope<TPayload> Create<TPayload>(
        TPayload payload,
        string source,
        Guid correlationId,
        Guid? causationId = null,
        Guid? tenantId = null,
        string? userId = null,
        string? eventType = null,
        int eventVersion = 1)
        where TPayload : IIntegrationEvent =>
        new()
        {
            EventType = eventType ?? typeof(TPayload).Name,
            EventVersion = eventVersion,
            Source = source,
            CorrelationId = correlationId,
            CausationId = causationId,
            TenantId = tenantId,
            UserId = userId,
            Payload = payload,
        };
}
