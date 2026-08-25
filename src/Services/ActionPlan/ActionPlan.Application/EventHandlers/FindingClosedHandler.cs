using ActionPlan.Application.Common;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Finding.Contracts.Events;

namespace ActionPlan.Application.EventHandlers;

/// <summary>
/// Marks the local <see cref="Domain.Entities.FindingReference"/> closed so
/// new action plans can no longer be created against that finding
/// (CLAUDE.md §5 — again, no FindingDb access; this is driven purely by the
/// FindingClosed integration event).
/// </summary>
public sealed class FindingClosedHandler : IIntegrationEventHandler<FindingClosed>
{
    private readonly IFindingReferenceRepository _findingReferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FindingClosedHandler(IFindingReferenceRepository findingReferenceRepository, IUnitOfWork unitOfWork)
    {
        _findingReferenceRepository = findingReferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EventEnvelope<FindingClosed> envelope, CancellationToken cancellationToken = default)
    {
        var reference = await _findingReferenceRepository.GetByIdAsync(envelope.Payload.FindingId, cancellationToken);
        if (reference is null)
        {
            // FindingCreated hasn't been consumed yet (event ordering isn't
            // guaranteed) — nothing local to update; see Finding Service's
            // identical AuditClosedHandler for the full rationale.
            return;
        }

        reference.MarkClosed();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
