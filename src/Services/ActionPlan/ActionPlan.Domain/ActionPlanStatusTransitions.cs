using ActionPlan.Domain.Enums;

namespace ActionPlan.Domain;

/// <summary>
/// Single source of truth for the action-plan state machine (CLAUDE.md §2).
/// Overdue is deliberately absent as a transition *target* here — it is
/// never user-initiated; see <see cref="Entities.ActionPlan.MarkOverdue"/>
/// and <see cref="CanBecomeOverdue"/> for how the system enters it.
/// </summary>
public static class ActionPlanStatusTransitions
{
    private static readonly IReadOnlyDictionary<ActionPlanStatus, ActionPlanStatus[]> Allowed = new Dictionary<ActionPlanStatus, ActionPlanStatus[]>
    {
        [ActionPlanStatus.Assigned] = [ActionPlanStatus.InProgress, ActionPlanStatus.Cancelled],
        [ActionPlanStatus.InProgress] = [ActionPlanStatus.SubmittedForApproval, ActionPlanStatus.Cancelled],
        [ActionPlanStatus.SubmittedForApproval] = [ActionPlanStatus.Approved, ActionPlanStatus.Rejected, ActionPlanStatus.Cancelled],
        [ActionPlanStatus.Rejected] = [ActionPlanStatus.InProgress, ActionPlanStatus.Cancelled],
        [ActionPlanStatus.Overdue] = [ActionPlanStatus.InProgress, ActionPlanStatus.Cancelled],
        [ActionPlanStatus.Approved] = [ActionPlanStatus.Closed, ActionPlanStatus.Cancelled],
        [ActionPlanStatus.Closed] = [],
        [ActionPlanStatus.Cancelled] = [],
    };

    /// <summary>
    /// Statuses in which the owner hasn't yet submitted for approval — the
    /// only ones a passed due date can escalate out of (CLAUDE.md §2:
    /// "unclosed actions past due date become Overdue"). Once submitted, the
    /// action is in the approver's queue, not the owner's, so overdue
    /// detection intentionally stops watching it.
    /// </summary>
    private static readonly ActionPlanStatus[] OverdueEligible =
    [
        ActionPlanStatus.Assigned,
        ActionPlanStatus.InProgress,
        ActionPlanStatus.Rejected,
    ];

    public static bool IsAllowed(ActionPlanStatus from, ActionPlanStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);

    public static bool CanBecomeOverdue(ActionPlanStatus status) => OverdueEligible.Contains(status);
}
