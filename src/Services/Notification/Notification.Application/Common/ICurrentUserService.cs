namespace Notification.Application.Common;

/// <summary>Identity of the caller, sourced only from validated Entra ID JWT claims (CLAUDE.md §10) — see Audit/Finding Service's identical interface for the full rationale.</summary>
public interface ICurrentUserService
{
    string UserId { get; }

    string TenantId { get; }
}
