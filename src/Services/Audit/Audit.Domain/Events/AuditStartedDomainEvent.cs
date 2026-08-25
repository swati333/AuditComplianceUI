using Ehs.SharedKernel.Domain;

namespace Audit.Domain.Events;

public sealed record AuditStartedDomainEvent(Guid AuditId, DateTime ActualStartDateUtc) : DomainEvent;
