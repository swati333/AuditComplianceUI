using Ehs.Contracts.Events;

namespace ActionPlan.Contracts.Events;

public sealed record ActionPlanStatusChanged(Guid ActionPlanId, Guid FindingId, string FromStatus, string ToStatus, DateTime ChangedAtUtc) : IIntegrationEvent;
