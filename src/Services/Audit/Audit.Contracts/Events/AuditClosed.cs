using Ehs.Contracts.Events;

namespace Audit.Contracts.Events;

public sealed record AuditClosed(Guid AuditId, DateTime ClosedAtUtc) : IIntegrationEvent;
