namespace Ehs.Observability.Security;

/// <summary>
/// Entra ID App Role values each <see cref="EhsAuthorizationPolicies"/> policy
/// maps to. These must be defined verbatim (Value field) as App Roles on the
/// platform's Entra App Registration and assigned to users/groups — see
/// docs/entra-id-setup.md. They arrive on the access token's <c>roles</c>
/// claim array, which the JWT bearer handler surfaces as one
/// <see cref="System.Security.Claims.ClaimTypes.Role"/> claim per role.
/// </summary>
public static class EntraAppRoles
{
    public const string AuditManage = "Audit.Manage";

    public const string AuditPerform = "Audit.Perform";

    public const string FindingManage = "Finding.Manage";

    public const string ActionManageOwn = "Action.ManageOwn";

    public const string ActionApprove = "Action.Approve";

    public const string ReportView = "Report.View";

    public const string ConfigurationManage = "Configuration.Manage";
}
