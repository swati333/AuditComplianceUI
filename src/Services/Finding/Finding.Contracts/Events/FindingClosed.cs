using Ehs.Contracts.Events;

namespace Finding.Contracts.Events;

public sealed record FindingClosed(Guid FindingId, Guid AuditId, DateTime ClosedAtUtc) : IIntegrationEvent;
