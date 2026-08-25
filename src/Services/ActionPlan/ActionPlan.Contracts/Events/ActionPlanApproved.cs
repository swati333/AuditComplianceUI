using Ehs.Contracts.Events;

namespace ActionPlan.Contracts.Events;

public sealed record ActionPlanApproved(Guid ActionPlanId, Guid FindingId, string ApprovedBy, DateTime ApprovedAtUtc) : IIntegrationEvent;
