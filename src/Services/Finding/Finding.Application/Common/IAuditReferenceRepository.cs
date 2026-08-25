using Finding.Domain.Entities;

namespace Finding.Application.Common;

public interface IAuditReferenceRepository
{
    Task<AuditReference?> GetByIdAsync(Guid auditId, CancellationToken cancellationToken = default);

    void Add(AuditReference reference);
}
