namespace Finding.Domain.Entities;

/// <summary>
/// Local, eventually-consistent projection of the audits Finding Service
/// knows about — populated exclusively by consuming Audit Service's
/// integration events (AuditCreated, AuditClosed), never by querying AuditDb
/// (CLAUDE.md §5: "a service never directly reads or writes another
/// service's tables"). Deliberately minimal: just enough to let Finding
/// creation reject an obviously-closed audit without a synchronous
/// cross-service call. Not an AuditableEntity — it isn't a business record
/// this service owns, it's a cache with a single source of truth elsewhere.
/// </summary>
public sealed class AuditReference
{
    public Guid AuditId { get; private set; }

    public string Title { get; private set; } = default!;

    public bool IsClosed { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private AuditReference()
    {
    }

    public static AuditReference Create(Guid auditId, string title) =>
        new()
        {
            AuditId = auditId,
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
