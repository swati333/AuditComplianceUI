using Finding.Application.Common;

namespace Finding.Infrastructure.Services;

/// <summary>
/// Answers "does this finding have a corrective action" from the local
/// <see cref="Finding.Domain.Entities.ActionPlanReference"/> projection,
/// which is kept in sync by inbox-consuming Action Plan Service's
/// ActionPlanAssigned integration event (see
/// Finding.Application.EventHandlers.ActionPlanAssignedHandler) — never by
/// querying ActionPlanDb directly (CLAUDE.md §5).
/// </summary>
public sealed class ActionPlanGateway : IActionPlanGateway
{
    private readonly IActionPlanReferenceRepository _actionPlanReferenceRepository;

    public ActionPlanGateway(IActionPlanReferenceRepository actionPlanReferenceRepository)
    {
        _actionPlanReferenceRepository = actionPlanReferenceRepository;
    }

    public Task<bool> HasCorrectiveActionAsync(Guid findingId, CancellationToken cancellationToken = default) =>
        _actionPlanReferenceRepository.ExistsAsync(findingId, cancellationToken);
}
