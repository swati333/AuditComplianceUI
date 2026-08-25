using Audit.Domain.Enums;

namespace Audit.Domain;

/// <summary>
/// Single source of truth for the audit state machine (CLAUDE.md §2):
/// Draft → Planned → InProgress → Completed → Closed, with cancellation
/// permitted only from the earlier, still-in-flight states.
/// </summary>
public static class AuditStatusTransitions
{
    private static readonly IReadOnlyDictionary<AuditStatus, AuditStatus[]> Allowed = new Dictionary<AuditStatus, AuditStatus[]>
    {
        [AuditStatus.Draft] = [AuditStatus.Planned, AuditStatus.Cancelled],
        [AuditStatus.Planned] = [AuditStatus.InProgress, AuditStatus.Cancelled],
        [AuditStatus.InProgress] = [AuditStatus.Completed, AuditStatus.Cancelled],
        [AuditStatus.Completed] = [AuditStatus.Closed],
        [AuditStatus.Closed] = [],
        [AuditStatus.Cancelled] = [],
    };

    public static bool IsAllowed(AuditStatus from, AuditStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}
