using Finding.Domain.Enums;

namespace Finding.Domain.Entities;

/// <summary>
/// Append-only record of a single status transition (CLAUDE.md §2). Not an
/// AuditableEntity — it's itself an immutable audit-trail row.
/// </summary>
public sealed class FindingStatusHistory
{
    public Guid Id { get; private set; }

    public Guid FindingId { get; private set; }

    public FindingStatus? FromStatus { get; private set; }

    public FindingStatus ToStatus { get; private set; }

    public string ChangedBy { get; private set; } = default!;

    public DateTime ChangedAtUtc { get; private set; }

    private FindingStatusHistory()
    {
    }

    internal static FindingStatusHistory Create(Guid findingId, FindingStatus? fromStatus, FindingStatus toStatus, string changedBy) =>
        new()
        {
            Id = Guid.NewGuid(),
            FindingId = findingId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedBy = changedBy,
            ChangedAtUtc = DateTime.UtcNow,
        };
}
