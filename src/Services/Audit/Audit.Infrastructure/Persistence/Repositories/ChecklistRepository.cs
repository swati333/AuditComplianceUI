using Audit.Application.Common;
using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Repositories;

public sealed class ChecklistRepository : IChecklistRepository
{
    private readonly AuditDbContext _dbContext;

    public ChecklistRepository(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Checklist checklist) => _dbContext.Checklists.Add(checklist);

    // Every current call site is read-only (existence checks, mandatory-question
    // lookups) — Checklist is never mutated from within the Audit Service.
    public Task<Checklist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Checklists
            .AsNoTracking()
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Checklist>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Checklists
            .AsNoTracking()
            .Include(c => c.Questions)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
}
