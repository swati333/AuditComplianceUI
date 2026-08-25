namespace ActionPlan.Application.Common;

/// <summary>
/// Identity of the caller, for CreatedBy/ModifiedBy/ChangedBy audit columns
/// and self-approval-prevention checks, sourced only from validated Entra ID
/// JWT claims (CLAUDE.md §10) — see Finding Service's identical interface.
/// </summary>
public interface ICurrentUserService
{
    string UserId { get; }

    string TenantId { get; }
}
