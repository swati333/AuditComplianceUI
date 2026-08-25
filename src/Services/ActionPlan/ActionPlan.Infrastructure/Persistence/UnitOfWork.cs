using ActionPlan.Application.Common;
using Ehs.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ActionPlan.Infrastructure.Persistence;

/// <summary>
/// Wraps <see cref="ActionPlanDbContext.SaveChangesAsync"/> and translates EF
/// Core's <see cref="DbUpdateConcurrencyException"/> into the shared
/// <see cref="ConflictException"/> (CLAUDE.md §4).
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ActionPlanDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(ActionPlanDbContext dbContext, ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Optimistic concurrency conflict while saving changes.");
            throw new ConflictException(
                "The record was modified by another user since it was last loaded. Reload and try again.",
                "CONCURRENCY_CONFLICT");
        }
    }
}
