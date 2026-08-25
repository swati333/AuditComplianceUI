namespace Finding.Application.Common;

/// <summary>
/// Identity of the caller, for CreatedBy/ModifiedBy/ChangedBy audit columns,
/// sourced only from validated Entra ID JWT claims (CLAUDE.md §10) — see
/// Audit Service's identical interface for the full rationale.
/// </summary>
public interface ICurrentUserService
{
    string UserId { get; }

    string TenantId { get; }
}
