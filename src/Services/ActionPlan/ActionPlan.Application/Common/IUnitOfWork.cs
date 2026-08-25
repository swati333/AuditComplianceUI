namespace ActionPlan.Application.Common;

/// <summary>
/// Commits the current unit of work. The Infrastructure implementation wraps
/// EF Core's <c>DbContext.SaveChangesAsync</c> and translates
/// <c>DbUpdateConcurrencyException</c> into <c>ConflictException</c>.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
