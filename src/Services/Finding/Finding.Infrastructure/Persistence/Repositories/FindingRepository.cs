using Finding.Application.Common;
using Finding.Contracts.Requests;
using Finding.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using FindingEntity = Finding.Domain.Entities.Finding;

namespace Finding.Infrastructure.Persistence.Repositories;

public sealed class FindingRepository : IFindingRepository
{
    private readonly FindingDbContext _dbContext;

    public FindingRepository(FindingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(FindingEntity finding) => _dbContext.Findings.Add(finding);

    public Task<FindingEntity?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Findings.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<FindingEntity?> GetTrackedWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Findings
            .Include(f => f.Comments)
            .Include(f => f.Documents)
            .Include(f => f.StatusHistory)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<FindingEntity?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Findings
            .AsNoTracking()
            .Include(f => f.Comments)
            .Include(f => f.Documents)
            .Include(f => f.StatusHistory)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<FindingEntity> Items, int TotalCount)> SearchAsync(FindingListQuery query, CancellationToken cancellationToken = default)
    {
        var filtered = _dbContext.Findings.AsNoTracking().AsQueryable();

        if (query.AuditId is { } auditId)
        {
            filtered = filtered.Where(f => f.AuditId == auditId);
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<FindingStatus>(query.Status, ignoreCase: true, out var status))
        {
            filtered = filtered.Where(f => f.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Severity) && Enum.TryParse<FindingSeverity>(query.Severity, ignoreCase: true, out var severity))
        {
            filtered = filtered.Where(f => f.Severity == severity);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var search = query.SearchText.Trim();
            filtered = filtered.Where(f => f.Title.Contains(search) || (f.Description != null && f.Description.Contains(search)));
        }

        var totalCount = await filtered.CountAsync(cancellationToken);

        var sorted = ApplySort(filtered, query.SortBy, query.SortDirection);

        var items = await sorted
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void SetRowVersion(FindingEntity finding, byte[] rowVersion) =>
        _dbContext.Entry(finding).Property(f => f.RowVersion).OriginalValue = rowVersion;

    /// <summary>Always appends a final <c>ThenBy(Id)</c> tie-breaker so paging is deterministic (CLAUDE.md §4).</summary>
    private static IOrderedQueryable<FindingEntity> ApplySort(IQueryable<FindingEntity> query, string? sortBy, string sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<FindingEntity> ordered = sortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(f => f.Title) : query.OrderBy(f => f.Title),
            "status" => descending ? query.OrderByDescending(f => f.Status) : query.OrderBy(f => f.Status),
            "severity" => descending ? query.OrderByDescending(f => f.Severity) : query.OrderBy(f => f.Severity),
            _ => descending ? query.OrderByDescending(f => f.CreatedDate) : query.OrderBy(f => f.CreatedDate),
        };

        return ordered.ThenBy(f => f.Id);
    }
}
