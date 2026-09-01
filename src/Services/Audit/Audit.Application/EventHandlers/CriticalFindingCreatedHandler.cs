using Audit.Application.Common;
using Audit.Domain.Entities;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Finding.Contracts.Events;

namespace Audit.Application.EventHandlers;

/// <summary>
/// Opens the local <see cref="OpenCriticalFinding"/> projection entry that
/// backs Audit.Close()'s "Critical findings are open" gate (CLAUDE.md §2).
/// </summary>
public sealed class CriticalFindingCreatedHandler : IIntegrationEventHandler<CriticalFindingCreated>
{
    private readonly IOpenCriticalFindingRepository _openCriticalFindingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriticalFindingCreatedHandler(IOpenCriticalFindingRepository openCriticalFindingRepository, IUnitOfWork unitOfWork)
    {
        _openCriticalFindingRepository = openCriticalFindingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EventEnvelope<CriticalFindingCreated> envelope, CancellationToken cancellationToken = default)
    {
        var existing = await _openCriticalFindingRepository.GetByFindingIdAsync(envelope.Payload.FindingId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        _openCriticalFindingRepository.Add(OpenCriticalFinding.Create(envelope.Payload.FindingId, envelope.Payload.AuditId));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
