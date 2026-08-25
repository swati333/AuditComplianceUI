using Ehs.SharedKernel.Domain;

namespace Audit.Domain.Events;

public sealed record AuditPlannedDomainEvent(Guid AuditId, DateTime PlannedStartDate, DateTime PlannedEndDate) : DomainEvent;
