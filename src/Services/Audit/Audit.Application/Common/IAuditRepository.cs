using Audit.Contracts.Requests;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.Application.Common;

public interface IAuditRepository
{
    /// <summary>Tracked, aggregate root only — for commands that don't need the owned collections.</summary>
    Task<AuditEntity?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Tracked, with team members / checklist responses / status history — for commands that need them.</summary>
    Task<AuditEntity?> GetTrackedWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>AsNoTracking, with owned collections — for the read-only GET-by-id endpoint.</summary>
    Task<AuditEntity?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>AsNoTracking — for the read-only list endpoint.</summary>
    Task<(IReadOnlyList<AuditEntity> Items, int TotalCount)> SearchAsync(AuditListQuery query, CancellationToken cancellationToken = default);

    void Add(AuditEntity audit);

    /// <summary>
    /// Overrides the tracked entity's original RowVersion with the value the
    /// client last read, so SaveChanges detects a conflict if another writer
    /// updated the row in between (true optimistic concurrency — see
    /// AuditRepository for why this can't just rely on the freshly-loaded value).
    /// </summary>
    void SetRowVersion(AuditEntity audit, byte[] rowVersion);
}
