using Ehs.Contracts.Events;

namespace Finding.Contracts.Events;

public sealed record FindingCreated(Guid FindingId, Guid AuditId, string Title, string Severity, DateTime CreatedAtUtc) : IIntegrationEvent;
