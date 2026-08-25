using Ehs.Contracts.Events;

namespace Notification.Contracts.ExternalEvents;

/// <summary>
/// Action Plan Service (CLAUDE.md phase 4) does not exist yet, so there is
/// no real Audit.Contracts-style project to reference. These are
/// Notification Service's own local view of the event shape it expects —
/// the standard "consumer defines its expected contract" pattern used when a
/// consumer is built before its producer. When Action Plan Service is
/// implemented, its own ActionPlan.Contracts project becomes the source of
/// truth and these should be replaced by a reference to it, the same way
/// Finding Service references Audit.Contracts today.
/// </summary>
public sealed record ActionPlanAssigned(Guid ActionPlanId, Guid FindingId, string AssignedToUserId, string Title, DateTime DueDate, DateTime AssignedAtUtc) : IIntegrationEvent;

public sealed record ActionPlanSubmitted(Guid ActionPlanId, Guid FindingId, string SubmittedByUserId, DateTime SubmittedAtUtc) : IIntegrationEvent;

/// <summary><see cref="AssignedToUserId"/> is who needs to act on the rejection (the notification recipient); <see cref="RejectedByUserId"/> is the approver, included for message content only.</summary>
public sealed record ActionPlanRejected(Guid ActionPlanId, Guid FindingId, string AssignedToUserId, string RejectedByUserId, string Reason, DateTime RejectedAtUtc) : IIntegrationEvent;

public sealed record ActionPlanOverdue(Guid ActionPlanId, Guid FindingId, string AssignedToUserId, DateTime DueDate, DateTime DetectedAtUtc) : IIntegrationEvent;
