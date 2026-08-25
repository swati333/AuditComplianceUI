using Ehs.SharedKernel.Domain;

namespace Finding.Domain.Events;

public sealed record FindingClosedDomainEvent(Guid FindingId, Guid AuditId, DateTime ClosedAtUtc) : DomainEvent;
