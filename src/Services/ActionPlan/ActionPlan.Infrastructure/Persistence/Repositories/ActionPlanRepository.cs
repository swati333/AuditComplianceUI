using ActionPlan.Application.Common;
using ActionPlan.Contracts.Requests;
using ActionPlan.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ActionPlanEntity = ActionPlan.Domain.Entities.ActionPlan;

namespace ActionPlan.Infrastructure.Persistence.Repositories;

public sealed class ActionPlanRepository : IActionPlanRepository
{
    private readonly ActionPlanDbContext _dbContext;

    public ActionPlanRepository(ActionPlanDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(ActionPlanEntity actionPlan) => _dbContext.ActionPlans.Add(actionPlan);

    public Task<ActionPlanEntity?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ActionPlans.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<ActionPlanEntity?> GetTrackedWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ActionPlans
            .Include(a => a.Comments)
            .Include(a => a.Evidence)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<ActionPlanEntity?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ActionPlans
            .AsNoTracking()
            .Include(a => a.Comments)
            .Include(a => a.Evidence)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<ActionPlanEntity> Items, int TotalCount)> SearchAsync(ActionPlanListQuery query, CancellationToken cancellationToken = default)
    {
        var filtered = _dbContext.ActionPlans.AsNoTracking().AsQueryable();

        if (query.FindingId is { } findingId)
        {
            filtered = filtered.Where(a => a.FindingId == findingId);
        }

        if (query.IsOverdue == true)
        {
            filtered = filtered.Where(a => a.Status == ActionPlanStatus.Overdue);
        }
        else if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<ActionPlanStatus>(query.Status, ignoreCase: true, out var status))
        {
            filtered = filtered.Where(a => a.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Priority) && Enum.TryParse<ActionPriority>(query.Priority, ignoreCase: true, out var priority))
        {
            filtered = filtered.Where(a => a.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerId))
        {
            filtered = filtered.Where(a => a.OwnerId == query.OwnerId);
        }

        if (!string.IsNullOrWhiteSpace(query.ApproverId))
        {
            filtered = filtered.Where(a => a.ApproverId == query.ApproverId);
        }

        if (query.DueBefore is { } dueBefore)
        {
            filtered = filtered.Where(a => a.DueDate <= dueBefore);
        }

        if (query.DueAfter is { } dueAfter)
        {
            filtered = filtered.Where(a => a.DueDate >= dueAfter);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var search = query.SearchText.Trim();
            filtered = filtered.Where(a => a.Title.Contains(search) || (a.Description != null && a.Description.Contains(search)));
        }

        var totalCount = await filtered.CountAsync(cancellationToken);

        var sorted = ApplySort(filtered, query.SortBy, query.SortDirection);

        var items = await sorted
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<ActionPlanEntity>> GetOverdueCandidatesAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.ActionPlans
            .Where(a => a.Status == ActionPlanStatus.Assigned || a.Status == ActionPlanStatus.InProgress || a.Status == ActionPlanStatus.Rejected)
            .ToListAsync(cancellationToken);

    public Task<bool> HasActiveActionPlanAsync(Guid findingId, CancellationToken cancellationToken = default) =>
        _dbContext.ActionPlans
            .AsNoTracking()
            .AnyAsync(a => a.FindingId == findingId && a.Status != ActionPlanStatus.Cancelled, cancellationToken);

    public void SetRowVersion(ActionPlanEntity actionPlan, byte[] rowVersion) =>
        _dbContext.Entry(actionPlan).Property(a => a.RowVersion).OriginalValue = rowVersion;

    /// <summary>Always appends a final <c>ThenBy(Id)</c> tie-breaker so paging is deterministic (CLAUDE.md §4).</summary>
    private static IOrderedQueryable<ActionPlanEntity> ApplySort(IQueryable<ActionPlanEntity> query, string? sortBy, string sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<ActionPlanEntity> ordered = sortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "status" => descending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "priority" => descending ? query.OrderByDescending(a => a.Priority) : query.OrderBy(a => a.Priority),
            "duedate" => descending ? query.OrderByDescending(a => a.DueDate) : query.OrderBy(a => a.DueDate),
            _ => descending ? query.OrderByDescending(a => a.CreatedDate) : query.OrderBy(a => a.CreatedDate),
        };

        return ordered.ThenBy(a => a.Id);
    }
}
