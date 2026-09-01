using Audit.Domain.Entities;

namespace Audit.Application.Common;

public interface IOpenRequiredActionRepository
{
    Task<bool> ExistsForAuditAsync(Guid auditId, CancellationToken cancellationToken = default);

    Task<OpenRequiredAction?> GetByActionPlanIdAsync(Guid actionPlanId, CancellationToken cancellationToken = default);

    void Add(OpenRequiredAction entry);

    void Remove(OpenRequiredAction entry);
}
