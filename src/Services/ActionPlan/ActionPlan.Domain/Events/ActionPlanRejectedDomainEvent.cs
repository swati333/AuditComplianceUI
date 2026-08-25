using Ehs.SharedKernel.Domain;

namespace ActionPlan.Domain.Events;

public sealed record ActionPlanRejectedDomainEvent(Guid ActionPlanId, Guid FindingId, string RejectedBy, string Reason, DateTime RejectedAtUtc) : DomainEvent;
