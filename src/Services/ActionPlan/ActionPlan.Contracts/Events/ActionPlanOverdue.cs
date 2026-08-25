using Ehs.Contracts.Events;

namespace ActionPlan.Contracts.Events;

public sealed record ActionPlanOverdue(Guid ActionPlanId, Guid FindingId, string OwnerId, DateTime DueDate, DateTime DetectedAtUtc) : IIntegrationEvent;
