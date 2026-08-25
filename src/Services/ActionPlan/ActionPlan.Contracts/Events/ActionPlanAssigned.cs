using Ehs.Contracts.Events;

namespace ActionPlan.Contracts.Events;

public sealed record ActionPlanAssigned(Guid ActionPlanId, Guid FindingId, string OwnerId, string ApproverId, DateTime DueDate, string Priority, DateTime AssignedAtUtc) : IIntegrationEvent;
