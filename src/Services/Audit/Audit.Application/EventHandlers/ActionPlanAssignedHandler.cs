using ActionPlan.Contracts.Events;
using Audit.Application.Common;
using Audit.Domain.Entities;
using Ehs.Contracts.Events;
using Ehs.EventBus;

namespace Audit.Application.EventHandlers;

/// <summary>
/// Opens the local <see cref="OpenRequiredAction"/> projection entry that
/// backs Audit.Close()'s "mandatory actions are open" gate (CLAUDE.md §2) —
/// but only when <see cref="FindingReference"/> shows the owning finding is
/// High/Critical (the only findings that can carry a mandatory action). An
/// action plan assigned to a Low/Medium finding is optional, not mandatory,
/// and is deliberately not tracked here.
/// </summary>
public sealed class ActionPlanAssignedHandler : IIntegrationEventHandler<ActionPlanAssigned>
{
    private readonly IFindingReferenceRepository _findingReferenceRepository;
    private readonly IOpenRequiredActionRepository _openRequiredActionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActionPlanAssignedHandler(
        IFindingReferenceRepository findingReferenceRepository,
        IOpenRequiredActionRepository openRequiredActionRepository,
        IUnitOfWork unitOfWork)
    {
        _findingReferenceRepository = findingReferenceRepository;
        _openRequiredActionRepository = openRequiredActionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EventEnvelope<ActionPlanAssigned> envelope, CancellationToken cancellationToken = default)
    {
        var findingReference = await _findingReferenceRepository.GetByFindingIdAsync(envelope.Payload.FindingId, cancellationToken);
        if (findingReference is null)
        {
            // Either a Low/Medium finding (never stored — see FindingCreatedHandler)
            // or FindingCreated hasn't been consumed yet; nothing mandatory to track.
            return;
        }

        var existing = await _openRequiredActionRepository.GetByActionPlanIdAsync(envelope.Payload.ActionPlanId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        _openRequiredActionRepository.Add(OpenRequiredAction.Create(
            envelope.Payload.ActionPlanId,
            envelope.Payload.FindingId,
            findingReference.AuditId));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
