using Ehs.SharedKernel.Domain;

namespace ActionPlan.Domain.Events;

public sealed record ActionPlanSubmittedDomainEvent(Guid ActionPlanId, Guid FindingId, string ApproverId, DateTime SubmittedAtUtc) : DomainEvent;
