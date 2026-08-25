namespace Audit.Application.Common;

/// <summary>
/// Commits the current unit of work. The Infrastructure implementation wraps
/// EF Core's <c>DbContext.SaveChangesAsync</c>, translates
/// <c>DbUpdateConcurrencyException</c> into <c>ConflictException</c>, and (via
/// the DbContext's own override) writes any pending outbox rows in the same
/// transaction as the business change.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
