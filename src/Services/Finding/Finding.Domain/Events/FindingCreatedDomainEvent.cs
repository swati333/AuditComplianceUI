using Ehs.SharedKernel.Domain;
using Finding.Domain.Enums;

namespace Finding.Domain.Events;

public sealed record FindingCreatedDomainEvent(Guid FindingId, Guid AuditId, string Title, FindingSeverity Severity, DateTime CreatedAtUtc) : DomainEvent;
