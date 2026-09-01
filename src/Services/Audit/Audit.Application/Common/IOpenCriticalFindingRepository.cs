using Audit.Domain.Entities;

namespace Audit.Application.Common;

public interface IOpenCriticalFindingRepository
{
    Task<bool> ExistsForAuditAsync(Guid auditId, CancellationToken cancellationToken = default);

    Task<OpenCriticalFinding?> GetByFindingIdAsync(Guid findingId, CancellationToken cancellationToken = default);

    void Add(OpenCriticalFinding entry);

    void Remove(OpenCriticalFinding entry);
}
