using Audit.Application.Common;
using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Repositories;

public sealed class OpenRequiredActionRepository : IOpenRequiredActionRepository
{
    private readonly AuditDbContext _dbContext;

    public OpenRequiredActionRepository(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsForAuditAsync(Guid auditId, CancellationToken cancellationToken = default) =>
        _dbContext.OpenRequiredActions.AnyAsync(a => a.AuditId == auditId, cancellationToken);

    public Task<OpenRequiredAction?> GetByActionPlanIdAsync(Guid actionPlanId, CancellationToken cancellationToken = default) =>
        _dbContext.OpenRequiredActions.FirstOrDefaultAsync(a => a.ActionPlanId == actionPlanId, cancellationToken);

    public void Add(OpenRequiredAction entry) => _dbContext.OpenRequiredActions.Add(entry);

    public void Remove(OpenRequiredAction entry) => _dbContext.OpenRequiredActions.Remove(entry);
}
