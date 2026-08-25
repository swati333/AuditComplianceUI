using Ehs.SharedKernel.Domain;

namespace ActionPlan.Domain.Events;

/// <summary>Raised only by <see cref="Entities.ActionPlan.MarkOverdue"/> — the system's own escalation detector, never a user action.</summary>
public sealed record ActionPlanOverdueDomainEvent(Guid ActionPlanId, Guid FindingId, string OwnerId, DateTime DueDate, DateTime DetectedAtUtc) : DomainEvent;
