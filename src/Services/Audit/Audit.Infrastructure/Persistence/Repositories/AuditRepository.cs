using Audit.Application.Common;
using Audit.Contracts.Requests;
using Audit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.Infrastructure.Persistence.Repositories;

public sealed class AuditRepository : IAuditRepository
{
    private readonly AuditDbContext _dbContext;

    public AuditRepository(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(AuditEntity audit) => _dbContext.Audits.Add(audit);

    public Task<AuditEntity?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Audits.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<AuditEntity?> GetTrackedWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Audits
            .Include(a => a.TeamMembers)
            .Include(a => a.ChecklistResponses)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<AuditEntity?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Audits
            .AsNoTracking()
            .Include(a => a.TeamMembers)
            .Include(a => a.ChecklistResponses)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<AuditEntity> Items, int TotalCount)> SearchAsync(AuditListQuery query, CancellationToken cancellationToken = default)
    {
        var filtered = _dbContext.Audits.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<AuditStatus>(query.Status, ignoreCase: true, out var status))
        {
            filtered = filtered.Where(a => a.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            filtered = filtered.Where(a => a.Location.Contains(query.Location));
        }

        if (query.PlannedStartDateFrom is { } from)
        {
            filtered = filtered.Where(a => a.PlannedStartDate >= from);
        }

        if (query.PlannedStartDateTo is { } to)
        {
            filtered = filtered.Where(a => a.PlannedStartDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var search = query.SearchText.Trim();
            filtered = filtered.Where(a => a.Title.Contains(search) || a.Scope.Contains(search) || a.Location.Contains(search));
        }

        var totalCount = await filtered.CountAsync(cancellationToken);

        var sorted = ApplySort(filtered, query.SortBy, query.SortDirection);

        var items = await sorted
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void SetRowVersion(AuditEntity audit, byte[] rowVersion) =>
        _dbContext.Entry(audit).Property(a => a.RowVersion).OriginalValue = rowVersion;

    /// <summary>
    /// Always appends a final <c>ThenBy(Id)</c> tie-breaker so paging is
    /// deterministic (CLAUDE.md §4) even when the requested sort key has
    /// duplicate values across rows.
    /// </summary>
    private static IOrderedQueryable<AuditEntity> ApplySort(IQueryable<AuditEntity> query, string? sortBy, string sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<AuditEntity> ordered = sortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "status" => descending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "location" => descending ? query.OrderByDescending(a => a.Location) : query.OrderBy(a => a.Location),
            "plannedstartdate" => descending ? query.OrderByDescending(a => a.PlannedStartDate) : query.OrderBy(a => a.PlannedStartDate),
            _ => descending ? query.OrderByDescending(a => a.CreatedDate) : query.OrderBy(a => a.CreatedDate),
        };

        return ordered.ThenBy(a => a.Id);
    }
}
