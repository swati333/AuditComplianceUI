using Audit.Contracts.Events;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Finding.Application.Common;

namespace Finding.Application.EventHandlers;

/// <summary>
/// Marks the local <see cref="Finding.Domain.Entities.AuditReference"/>
/// closed so new findings can no longer be created against that audit
/// (CLAUDE.md §5 — again, no AuditDb access; this is driven purely by the
/// AuditClosed integration event).
/// </summary>
public sealed class AuditClosedHandler : IIntegrationEventHandler<AuditClosed>
{
    private readonly IAuditReferenceRepository _auditReferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuditClosedHandler(IAuditReferenceRepository auditReferenceRepository, IUnitOfWork unitOfWork)
    {
        _auditReferenceRepository = auditReferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EventEnvelope<AuditClosed> envelope, CancellationToken cancellationToken = default)
    {
        var reference = await _auditReferenceRepository.GetByIdAsync(envelope.Payload.AuditId, cancellationToken);
        if (reference is null)
        {
            // AuditCreated hasn't been consumed yet (event ordering isn't
            // guaranteed) — nothing local to update; the eventual AuditCreated
            // consumption will create a reference which is then stale-closed.
            // Acceptable for this phase; a real deployment would use a
            // durable subscription that preserves per-aggregate order.
            return;
        }

        reference.MarkClosed();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
