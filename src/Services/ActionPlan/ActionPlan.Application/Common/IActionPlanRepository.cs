using ActionPlan.Contracts.Requests;
using ActionPlanEntity = ActionPlan.Domain.Entities.ActionPlan;

namespace ActionPlan.Application.Common;

public interface IActionPlanRepository
{
    /// <summary>Tracked, aggregate root only — for commands that don't need the owned collections.</summary>
    Task<ActionPlanEntity?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Tracked, with comments/evidence/status history — for commands that need them.</summary>
    Task<ActionPlanEntity?> GetTrackedWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>AsNoTracking, with owned collections — for the read-only GET-by-id endpoint.</summary>
    Task<ActionPlanEntity?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>AsNoTracking — for the read-only list endpoint.</summary>
    Task<(IReadOnlyList<ActionPlanEntity> Items, int TotalCount)> SearchAsync(ActionPlanListQuery query, CancellationToken cancellationToken = default);

    /// <summary>AsNoTracking, unpaged — every action plan eligible to be checked for overdue escalation. Used only by OverdueDetectionService.</summary>
    Task<IReadOnlyList<ActionPlanEntity>> GetOverdueCandidatesAsync(CancellationToken cancellationToken = default);

    /// <summary>Whether the given finding has at least one non-cancelled action plan — backs Finding Service's "High/Critical requires a corrective action" gate (CLAUDE.md §2) via IActionPlanGateway once that's wired up.</summary>
    Task<bool> HasActiveActionPlanAsync(Guid findingId, CancellationToken cancellationToken = default);

    void Add(ActionPlanEntity actionPlan);

    /// <summary>Overrides the tracked entity's original RowVersion with the value the client last read, for true optimistic concurrency.</summary>
    void SetRowVersion(ActionPlanEntity actionPlan, byte[] rowVersion);
}
