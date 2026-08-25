namespace Audit.Domain.Enums;

/// <summary>
/// Audit lifecycle per CLAUDE.md §2: Draft → Planned → InProgress → Completed
/// → Closed, with cancellation permitted from the earlier, in-flight states.
/// </summary>
public enum AuditStatus
{
    Draft = 0,
    Planned = 1,
    InProgress = 2,
    Completed = 3,
    Closed = 4,
    Cancelled = 5,
}
