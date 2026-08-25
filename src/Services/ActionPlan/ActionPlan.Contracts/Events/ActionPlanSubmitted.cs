using Ehs.Contracts.Events;

namespace ActionPlan.Contracts.Events;

public sealed record ActionPlanSubmitted(Guid ActionPlanId, Guid FindingId, string ApproverId, DateTime SubmittedAtUtc) : IIntegrationEvent;
