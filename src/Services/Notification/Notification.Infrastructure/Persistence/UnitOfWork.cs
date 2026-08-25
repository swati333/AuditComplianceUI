using Ehs.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Application.Common;

namespace Notification.Infrastructure.Persistence;

/// <summary>Wraps SaveChangesAsync and translates DbUpdateConcurrencyException into ConflictException (CLAUDE.md §4) — same pattern as Audit/Finding Service.</summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly NotificationDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(NotificationDbContext dbContext, ILogger<UnitOfWork> logger)
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
