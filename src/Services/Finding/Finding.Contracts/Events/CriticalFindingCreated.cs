using Ehs.Contracts.Events;

namespace Finding.Contracts.Events;

public sealed record CriticalFindingCreated(Guid FindingId, Guid AuditId, string Title, DateTime CreatedAtUtc) : IIntegrationEvent;
