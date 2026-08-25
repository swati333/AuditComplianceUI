using Ehs.SharedKernel.Domain;

namespace Audit.Domain.Events;

public sealed record AuditClosedDomainEvent(Guid AuditId, DateTime ClosedAtUtc) : DomainEvent;
