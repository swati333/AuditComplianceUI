using ActionPlan.Domain.Enums;
using Ehs.SharedKernel.Domain;

namespace ActionPlan.Domain.Events;

/// <summary>
/// Generic transition event, raised for status changes that don't already
/// have a more specific event of their own — Start (→InProgress) and Close/
/// Cancel (→Closed/Cancelled). Submit/Approve/Reject/the system's Overdue
/// detection raise their own named events instead (CLAUDE.md §7's explicit
/// event list), since those carry (or downstream consumers care about)
/// details this generic shape doesn't.
/// </summary>
public sealed record ActionPlanStatusChangedDomainEvent(
    Guid ActionPlanId,
    Guid FindingId,
    ActionPlanStatus FromStatus,
    ActionPlanStatus ToStatus,
    DateTime ChangedAtUtc) : DomainEvent;
