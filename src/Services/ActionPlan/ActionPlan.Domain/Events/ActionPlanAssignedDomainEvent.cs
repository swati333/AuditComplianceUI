using ActionPlan.Domain.Enums;
using Ehs.SharedKernel.Domain;

namespace ActionPlan.Domain.Events;

public sealed record ActionPlanAssignedDomainEvent(
    Guid ActionPlanId,
    Guid FindingId,
    string OwnerId,
    string ApproverId,
    DateTime DueDate,
    ActionPriority Priority,
    DateTime AssignedAtUtc) : DomainEvent;
