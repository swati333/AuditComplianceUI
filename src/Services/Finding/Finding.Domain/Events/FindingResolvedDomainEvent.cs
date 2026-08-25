using Ehs.SharedKernel.Domain;

namespace Finding.Domain.Events;

public sealed record FindingResolvedDomainEvent(Guid FindingId, Guid AuditId, DateTime ResolvedAtUtc) : DomainEvent;
