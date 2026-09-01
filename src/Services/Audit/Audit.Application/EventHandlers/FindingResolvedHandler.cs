using Audit.Application.Common;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Finding.Contracts.Events;

namespace Audit.Application.EventHandlers;

/// <summary>
/// Closes the local <see cref="Audit.Domain.Entities.OpenCriticalFinding"/>
/// projection entry, if one is open for this finding — the finding lifecycle
/// (Open→...→Resolved→Verified→Closed) guarantees FindingResolved fires
/// before FindingClosed, so this is the single point where a Critical
/// finding stops counting against Audit.Close()'s gate (CLAUDE.md §2).
/// </summary>
public sealed class FindingResolvedHandler : IIntegrationEventHandler<FindingResolved>
{
    private readonly IOpenCriticalFindingRepository _openCriticalFindingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FindingResolvedHandler(IOpenCriticalFindingRepository openCriticalFindingRepository, IUnitOfWork unitOfWork)
    {
        _openCriticalFindingRepository = openCriticalFindingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EventEnvelope<FindingResolved> envelope, CancellationToken cancellationToken = default)
    {
        var entry = await _openCriticalFindingRepository.GetByFindingIdAsync(envelope.Payload.FindingId, cancellationToken);
        if (entry is null)
        {
            // Not a Critical finding (no OpenCriticalFinding was ever opened for
            // it), or CriticalFindingCreated hasn't been consumed yet — nothing
            // local to close in either case.
            return;
        }

        _openCriticalFindingRepository.Remove(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
