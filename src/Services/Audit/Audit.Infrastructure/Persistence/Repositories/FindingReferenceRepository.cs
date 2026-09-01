using Audit.Application.Common;
using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Repositories;

public sealed class FindingReferenceRepository : IFindingReferenceRepository
{
    private readonly AuditDbContext _dbContext;

    public FindingReferenceRepository(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FindingReference?> GetByFindingIdAsync(Guid findingId, CancellationToken cancellationToken = default) =>
        _dbContext.FindingReferences.FirstOrDefaultAsync(r => r.FindingId == findingId, cancellationToken);

    public void Add(FindingReference reference) => _dbContext.FindingReferences.Add(reference);
}
