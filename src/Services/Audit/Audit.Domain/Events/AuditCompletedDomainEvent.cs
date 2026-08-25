using Ehs.SharedKernel.Domain;

namespace Audit.Domain.Events;

public sealed record AuditCompletedDomainEvent(Guid AuditId, DateTime ActualEndDateUtc) : DomainEvent;
