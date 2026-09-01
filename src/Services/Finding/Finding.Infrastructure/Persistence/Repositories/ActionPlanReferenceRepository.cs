using Finding.Application.Common;
using Finding.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finding.Infrastructure.Persistence.Repositories;

public sealed class ActionPlanReferenceRepository : IActionPlanReferenceRepository
{
    private readonly FindingDbContext _dbContext;

    public ActionPlanReferenceRepository(FindingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid findingId, CancellationToken cancellationToken = default) =>
        _dbContext.ActionPlanReferences.AnyAsync(r => r.FindingId == findingId, cancellationToken);

    public void Add(ActionPlanReference reference) => _dbContext.ActionPlanReferences.Add(reference);
}
