namespace Audit.Application.Common;

/// <summary>
/// Identity of the caller, for CreatedBy/ModifiedBy/ChangedBy audit columns,
/// sourced only from validated Entra ID JWT claims (CLAUDE.md §10) — never
/// from a client-supplied header or request-body field. See Audit.Api's
/// CurrentUserService for the claim mapping (<c>oid</c>/<c>tid</c>).
/// </summary>
public interface ICurrentUserService
{
    string UserId { get; }

    string TenantId { get; }
}
