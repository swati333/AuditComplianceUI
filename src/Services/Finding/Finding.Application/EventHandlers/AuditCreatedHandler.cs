using Audit.Contracts.Events;
using Ehs.Contracts.Events;
using Ehs.EventBus;
using Finding.Application.Common;
using Finding.Domain.Entities;

namespace Finding.Application.EventHandlers;

/// <summary>
/// Populates the local <see cref="AuditReference"/> projection so
/// finding creation can validate an AuditId without ever querying AuditDb
/// (CLAUDE.md §5). Idempotency (redelivery must not duplicate the row) is
/// the caller's responsibility — see Finding.Infrastructure's
/// IntegrationEventConsumer, which checks the Inbox before invoking any handler.
/// </summary>
public sealed class AuditCreatedHandler : IIntegrationEventHandler<AuditCreated>
{
    private readonly IAuditReferenceRepository _auditReferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuditCreatedHandler(IAuditReferenceRepository auditReferenceRepository, IUnitOfWork unitOfWork)
    {
        _auditReferenceRepository = auditReferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EventEnvelope<AuditCreated> envelope, CancellationToken cancellationToken = default)
    {
        var existing = await _auditReferenceRepository.GetByIdAsync(envelope.Payload.AuditId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        _auditReferenceRepository.Add(AuditReference.Create(envelope.Payload.AuditId, envelope.Payload.Title));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
