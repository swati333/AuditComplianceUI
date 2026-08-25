namespace ActionPlan.Domain.Enums;

/// <summary>
/// Lifecycle per CLAUDE.md §2: Assigned → InProgress → SubmittedForApproval →
/// Approved → Closed, with Rejected/Overdue/Cancelled branches. Rejected
/// actions return to InProgress via Start() (CLAUDE.md §2's "Rejected
/// actions return to In Progress" rule) rather than skipping the Rejected
/// status entirely — that keeps a distinct, visible state for the owner's
/// "Rejected actions" queue. Overdue is a system-detected side-state, never
/// entered by a user action — see ActionPlanStatusTransitions and
/// ActionPlan.MarkOverdue.
/// </summary>
public enum ActionPlanStatus
{
    Assigned = 0,
    InProgress = 1,
    SubmittedForApproval = 2,
    Approved = 3,
    Closed = 4,
    Rejected = 5,
    Overdue = 6,
    Cancelled = 7,
}
