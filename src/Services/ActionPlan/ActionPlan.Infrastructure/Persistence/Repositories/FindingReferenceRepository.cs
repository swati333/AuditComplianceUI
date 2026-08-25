using ActionPlan.Application.Common;
using ActionPlan.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ActionPlan.Infrastructure.Persistence.Repositories;

public sealed class FindingReferenceRepository : IFindingReferenceRepository
{
    private readonly ActionPlanDbContext _dbContext;

    public FindingReferenceRepository(ActionPlanDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FindingReference?> GetByIdAsync(Guid findingId, CancellationToken cancellationToken = default) =>
        _dbContext.FindingReferences.FirstOrDefaultAsync(r => r.FindingId == findingId, cancellationToken);

    public void Add(FindingReference reference) => _dbContext.FindingReferences.Add(reference);
}
