using Ehs.SharedKernel.Domain;

namespace Finding.Domain.Events;

/// <summary>
/// Raised alongside <see cref="FindingCreatedDomainEvent"/>, only when a
/// finding is created directly at Critical severity — CLAUDE.md §2 requires
/// this to "trigger immediate notification." Not re-raised if a finding's
/// severity is later edited up to Critical (see Finding.UpdateDetails).
/// </summary>
public sealed record CriticalFindingCreatedDomainEvent(Guid FindingId, Guid AuditId, string Title, DateTime CreatedAtUtc) : DomainEvent;
