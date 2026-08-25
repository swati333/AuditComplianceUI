namespace Ehs.SharedKernel.Domain;

/// <summary>
/// Base type for mutable, persisted entities. Standardizes the audit columns,
/// optimistic-concurrency token and soft-delete flag required across every
/// microservice database per CLAUDE.md §5.
/// </summary>
public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : notnull
{
    public string CreatedBy { get; set; } = default!;

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
