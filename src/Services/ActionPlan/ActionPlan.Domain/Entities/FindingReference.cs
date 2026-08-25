namespace ActionPlan.Domain.Entities;

/// <summary>
/// Local, eventually-consistent projection of the findings Action Plan
/// Service knows about — populated exclusively by consuming Finding
/// Service's integration events (FindingCreated, FindingClosed), never by
/// querying FindingDb (CLAUDE.md §5: "a service never directly reads or
/// writes another service's tables"). Mirrors Finding Service's own
/// AuditReference pattern. Not an AuditableEntity — it's a cache with a
/// single source of truth elsewhere.
/// </summary>
public sealed class FindingReference
{
    public Guid FindingId { get; private set; }

    public string Title { get; private set; } = default!;

    public bool IsClosed { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private FindingReference()
    {
    }

    public static FindingReference Create(Guid findingId, string title) =>
        new()
        {
            FindingId = findingId,
            Title = title,
            IsClosed = false,
            UpdatedAtUtc = DateTime.UtcNow,
        };

    public void MarkClosed()
    {
        IsClosed = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
