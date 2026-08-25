namespace Ehs.Observability.Security;

/// <summary>
/// The 7 authorization policy names required by CLAUDE.md §10. Shared here so
/// every service's controllers, tests and DI wiring reference the same
/// string constants rather than re-typing policy names that could drift.
/// </summary>
public static class EhsAuthorizationPolicies
{
    public const string CanManageAudits = nameof(CanManageAudits);

    public const string CanPerformAudits = nameof(CanPerformAudits);

    public const string CanManageFindings = nameof(CanManageFindings);

    public const string CanManageOwnActions = nameof(CanManageOwnActions);

    public const string CanApproveActions = nameof(CanApproveActions);

    public const string CanViewReports = nameof(CanViewReports);

    public const string CanManageConfiguration = nameof(CanManageConfiguration);
}
