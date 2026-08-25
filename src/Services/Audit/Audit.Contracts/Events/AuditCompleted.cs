using Ehs.Contracts.Events;

namespace Audit.Contracts.Events;

public sealed record AuditCompleted(Guid AuditId, DateTime ActualEndDateUtc) : IIntegrationEvent;
