using ActionPlan.Domain.Enums;

namespace ActionPlan.Domain.Entities;

/// <summary>
/// Append-only record of a single status transition (CLAUDE.md §2). Not an
/// AuditableEntity — it's itself an immutable audit-trail row.
/// </summary>
public sealed class ActionPlanStatusHistory
{
    public Guid Id { get; private set; }

    public Guid ActionPlanId { get; private set; }

    public ActionPlanStatus? FromStatus { get; private set; }

    public ActionPlanStatus ToStatus { get; private set; }

    public string ChangedBy { get; private set; } = default!;

    public DateTime ChangedAtUtc { get; private set; }

    /// <summary>Populated only for the transition into Rejected — the mandatory rejection reason.</summary>
    public string? Reason { get; private set; }

    private ActionPlanStatusHistory()
    {
    }

    internal static ActionPlanStatusHistory Create(Guid actionPlanId, ActionPlanStatus? fromStatus, ActionPlanStatus toStatus, string changedBy, string? reason = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            ActionPlanId = actionPlanId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedBy = changedBy,
            ChangedAtUtc = DateTime.UtcNow,
            Reason = reason,
        };
}
