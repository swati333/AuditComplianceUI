using Audit.Domain.Enums;

namespace Audit.Domain.Entities;

/// <summary>
/// Append-only record of a single status transition (CLAUDE.md §2: "Every
/// status transition must be recorded in a status-history table"). Not an
/// AuditableEntity — it is itself an immutable audit-trail row, not a mutable
/// business record, so it carries no RowVersion/soft-delete of its own.
/// </summary>
public sealed class AuditStatusHistory
{
    public Guid Id { get; private set; }

    public Guid AuditId { get; private set; }

    public AuditStatus? FromStatus { get; private set; }

    public AuditStatus ToStatus { get; private set; }

    public string ChangedBy { get; private set; } = default!;

    public DateTime ChangedAtUtc { get; private set; }

    public string? Reason { get; private set; }

    private AuditStatusHistory()
    {
    }

    internal static AuditStatusHistory Create(Guid auditId, AuditStatus? fromStatus, AuditStatus toStatus, string changedBy, string? reason = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            AuditId = auditId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedBy = changedBy,
            ChangedAtUtc = DateTime.UtcNow,
            Reason = reason,
        };
}
