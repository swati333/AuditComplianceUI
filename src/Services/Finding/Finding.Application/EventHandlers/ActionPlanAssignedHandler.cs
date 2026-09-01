using ActionPlan.Contracts.Events;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Finding.Application.Common;
using Finding.Domain.Entities;

namespace Finding.Application.EventHandlers;

/// <summary>
/// Populates the local <see cref="ActionPlanReference"/> projection so
/// Finding.Resolve() can check "does this finding have a corrective action"
/// (CLAUDE.md §2) without ever querying ActionPlanDb (CLAUDE.md §5).
/// Idempotency is the caller's responsibility — see Finding.Infrastructure's
/// IntegrationEventConsumer, which checks the Inbox before invoking any
/// handler; this also guards against redelivery on its own by checking for
/// an existing row, matching the AuditCreatedHandler pattern.
/// </summary>
public sealed class ActionPlanAssignedHandler : IIntegrationEventHandler<ActionPlanAssigned>
{
    private readonly IActionPlanReferenceRepository _actionPlanReferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActionPlanAssignedHandler(IActionPlanReferenceRepository actionPlanReferenceRepository, IUnitOfWork unitOfWork)
    {
        _actionPlanReferenceRepository = actionPlanReferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EventEnvelope<ActionPlanAssigned> envelope, CancellationToken cancellationToken = default)
    {
        var exists = await _actionPlanReferenceRepository.ExistsAsync(envelope.Payload.FindingId, cancellationToken);
        if (exists)
        {
            return;
        }

        _actionPlanReferenceRepository.Add(ActionPlanReference.Create(envelope.Payload.FindingId, envelope.Payload.ActionPlanId));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
