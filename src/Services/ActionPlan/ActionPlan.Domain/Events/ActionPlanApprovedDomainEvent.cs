using Ehs.SharedKernel.Domain;

namespace ActionPlan.Domain.Events;

public sealed record ActionPlanApprovedDomainEvent(Guid ActionPlanId, Guid FindingId, string ApprovedBy, DateTime ApprovedAtUtc) : DomainEvent;
