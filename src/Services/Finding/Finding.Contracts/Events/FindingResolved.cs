using Ehs.Contracts.Events;

namespace Finding.Contracts.Events;

public sealed record FindingResolved(Guid FindingId, Guid AuditId, DateTime ResolvedAtUtc) : IIntegrationEvent;
