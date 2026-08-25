using Finding.Contracts.Requests;
using FindingEntity = Finding.Domain.Entities.Finding;

namespace Finding.Application.Common;

public interface IFindingRepository
{
    /// <summary>Tracked, aggregate root only — for commands that don't need the owned collections.</summary>
    Task<FindingEntity?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Tracked, with comments/documents/status history — for commands that need them.</summary>
    Task<FindingEntity?> GetTrackedWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>AsNoTracking, with owned collections — for the read-only GET-by-id endpoint.</summary>
    Task<FindingEntity?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>AsNoTracking — for the read-only list endpoint.</summary>
    Task<(IReadOnlyList<FindingEntity> Items, int TotalCount)> SearchAsync(FindingListQuery query, CancellationToken cancellationToken = default);

    void Add(FindingEntity finding);

    /// <summary>Overrides the tracked entity's original RowVersion with the value the client last read, for true optimistic concurrency.</summary>
    void SetRowVersion(FindingEntity finding, byte[] rowVersion);
}
