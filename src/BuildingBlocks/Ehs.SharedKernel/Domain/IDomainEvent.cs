namespace Ehs.SharedKernel.Domain;

/// <summary>
/// Marker abstraction for in-process domain events raised by an aggregate/entity
/// as a side effect of a state change. Distinct from <c>IIntegrationEvent</c> in
/// Ehs.Contracts, which crosses service boundaries.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTime OccurredOnUtc { get; }
}
