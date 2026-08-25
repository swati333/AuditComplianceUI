namespace Ehs.Contracts.Events;

/// <summary>
/// Marker for the payload of a cross-service integration event (e.g.
/// AuditCreated, CriticalFindingCreated). Concrete events are defined in each
/// service's own *.Contracts project, never here — this project only defines
/// the reusable envelope/versioning shape all of them travel in.
/// </summary>
public interface IIntegrationEvent
{
}
