using Ehs.Contracts.Events;

namespace Audit.Contracts.Events;

public sealed record AuditStarted(Guid AuditId, DateTime ActualStartDateUtc) : IIntegrationEvent;
