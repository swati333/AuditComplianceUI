using Ehs.Contracts.Events;

namespace ActionPlan.Contracts.Events;

public sealed record ActionPlanRejected(Guid ActionPlanId, Guid FindingId, string RejectedBy, string Reason, DateTime RejectedAtUtc) : IIntegrationEvent;
