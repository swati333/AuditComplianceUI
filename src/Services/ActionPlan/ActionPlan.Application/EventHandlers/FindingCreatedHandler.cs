using ActionPlan.Application.Common;
using ActionPlan.Domain.Entities;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Finding.Contracts.Events;

namespace ActionPlan.Application.EventHandlers;

/// <summary>
/// Populates the local <see cref="FindingReference"/> projection so action
/// plan creation can validate a FindingId without ever querying FindingDb
/// (CLAUDE.md §5). Idempotency (redelivery must not duplicate the row) is
/// the caller's responsibility — see Infrastructure's
/// IntegrationEventConsumer, which checks the Inbox before invoking any handler.
/// </summary>
public sealed class FindingCreatedHandler : IIntegrationEventHandler<FindingCreated>
{
    private readonly IFindingReferenceRepository _findingReferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FindingCreatedHandler(IFindingReferenceRepository findingReferenceRepository, IUnitOfWork unitOfWork)
    {
        _findingReferenceRepository = findingReferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EventEnvelope<FindingCreated> envelope, CancellationToken cancellationToken = default)
    {
        var existing = await _findingReferenceRepository.GetByIdAsync(envelope.Payload.FindingId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        _findingReferenceRepository.Add(FindingReference.Create(envelope.Payload.FindingId, envelope.Payload.Title));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
