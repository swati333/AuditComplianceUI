using Finding.Domain.Enums;

namespace Finding.Domain;

/// <summary>
/// Single source of truth for the finding state machine (CLAUDE.md §2):
/// Open → UnderReview → ActionRequired → Resolved → Verified → Closed.
/// Strictly linear — no branch, no cancellation, no skipping a stage.
/// </summary>
public static class FindingStatusTransitions
{
    private static readonly IReadOnlyDictionary<FindingStatus, FindingStatus[]> Allowed = new Dictionary<FindingStatus, FindingStatus[]>
    {
        [FindingStatus.Open] = [FindingStatus.UnderReview],
        [FindingStatus.UnderReview] = [FindingStatus.ActionRequired],
        [FindingStatus.ActionRequired] = [FindingStatus.Resolved],
        [FindingStatus.Resolved] = [FindingStatus.Verified],
        [FindingStatus.Verified] = [FindingStatus.Closed],
        [FindingStatus.Closed] = [],
    };

    public static bool IsAllowed(FindingStatus from, FindingStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}
