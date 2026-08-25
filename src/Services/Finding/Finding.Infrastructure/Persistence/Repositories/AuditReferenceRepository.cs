using Finding.Application.Common;
using Finding.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finding.Infrastructure.Persistence.Repositories;

public sealed class AuditReferenceRepository : IAuditReferenceRepository
{
    private readonly FindingDbContext _dbContext;

    public AuditReferenceRepository(FindingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AuditReference?> GetByIdAsync(Guid auditId, CancellationToken cancellationToken = default) =>
        _dbContext.AuditReferences.FirstOrDefaultAsync(r => r.AuditId == auditId, cancellationToken);

    public void Add(AuditReference reference) => _dbContext.AuditReferences.Add(reference);
}
