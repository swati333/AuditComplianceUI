using Audit.Application.Common;
using Audit.Domain.Entities;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Finding.Contracts.Events;

namespace Audit.Application.EventHandlers;

/// <summary>
/// Populates the local <see cref="FindingReference"/> projection, but only
/// for High/Critical findings — those are the only ones that can carry a
/// "mandatory" action (CLAUDE.md §2), and this projection exists solely to
/// let a later ActionPlanAssigned/ActionPlanStatusChanged event (which
/// carries FindingId but not AuditId) be attributed to the right audit for
/// <see cref="OpenRequiredAction"/> tracking. Low/Medium findings are
/// intentionally not stored.
/// </summary>
public sealed class FindingCreatedHandler : IIntegrationEventHandler<FindingCreated>
{
    private static readonly string[] MandatorySeverities = ["High", "Critical"];

    private readonly IFindingReferenceRepository _findingReferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FindingCreatedHandler(IFindingReferenceRepository findingReferenceRepository, IUnitOfWork unitOfWork)
    {
        _findingReferenceRepository = findingReferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EventEnvelope<FindingCreated> envelope, CancellationToken cancellationToken = default)
    {
        if (!MandatorySeverities.Contains(envelope.Payload.Severity, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var existing = await _findingReferenceRepository.GetByFindingIdAsync(envelope.Payload.FindingId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        _findingReferenceRepository.Add(FindingReference.Create(envelope.Payload.FindingId, envelope.Payload.AuditId, envelope.Payload.Severity));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
