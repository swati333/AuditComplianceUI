using Ehs.SharedKernel.Domain;

namespace Audit.Domain.Events;

/// <summary>
/// In-process notification that a new audit was created. Infrastructure maps
/// this 1:1 onto the wire-level <c>AuditCreated</c> integration event
/// (Audit.Contracts.Events) when writing the transactional outbox — kept as
/// a distinct type so Domain never depends on Ehs.Contracts.
/// </summary>
public sealed record AuditCreatedDomainEvent(Guid AuditId, string Title, string Scope, string Location, DateTime CreatedAtUtc) : DomainEvent;
