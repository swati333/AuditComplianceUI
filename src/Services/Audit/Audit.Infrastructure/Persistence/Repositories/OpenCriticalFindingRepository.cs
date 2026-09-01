using Audit.Application.Common;
using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Repositories;

public sealed class OpenCriticalFindingRepository : IOpenCriticalFindingRepository
{
    private readonly AuditDbContext _dbContext;

    public OpenCriticalFindingRepository(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsForAuditAsync(Guid auditId, CancellationToken cancellationToken = default) =>
        _dbContext.OpenCriticalFindings.AnyAsync(f => f.AuditId == auditId, cancellationToken);

    public Task<OpenCriticalFinding?> GetByFindingIdAsync(Guid findingId, CancellationToken cancellationToken = default) =>
        _dbContext.OpenCriticalFindings.FirstOrDefaultAsync(f => f.FindingId == findingId, cancellationToken);

    public void Add(OpenCriticalFinding entry) => _dbContext.OpenCriticalFindings.Add(entry);

    public void Remove(OpenCriticalFinding entry) => _dbContext.OpenCriticalFindings.Remove(entry);
}
