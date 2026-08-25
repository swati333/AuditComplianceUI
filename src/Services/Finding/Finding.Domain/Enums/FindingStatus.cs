namespace Finding.Domain.Enums;

/// <summary>
/// Strictly linear lifecycle per CLAUDE.md §2 — unlike Audit, Finding has no
/// cancellation branch: Open → UnderReview → ActionRequired → Resolved →
/// Verified → Closed.
/// </summary>
public enum FindingStatus
{
    Open = 0,
    UnderReview = 1,
    ActionRequired = 2,
    Resolved = 3,
    Verified = 4,
    Closed = 5,
}
