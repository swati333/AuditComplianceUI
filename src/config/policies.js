/**
 * Mirrors Ehs.Observability.Security.EhsAuthorizationPolicies /
 * EntraAppRoles on the backend (CLAUDE.md §10) — one source of truth for
 * policy names, mapped onto the same Entra App Role values that land in the
 * access token's `roles` claim. The React app never invents its own
 * authorization semantics; it only reflects what each API already enforces,
 * so a hidden nav item or disabled button is a UX convenience, not a
 * security boundary — the API is the real gate.
 */
export const Policy = {
    CanManageAudits: 'CanManageAudits',
    CanPerformAudits: 'CanPerformAudits',
    CanManageFindings: 'CanManageFindings',
    CanManageOwnActions: 'CanManageOwnActions',
    CanApproveActions: 'CanApproveActions',
    CanViewReports: 'CanViewReports',
    CanManageConfiguration: 'CanManageConfiguration',
};
export const EntraAppRole = {
    AuditManage: 'Audit.Manage',
    AuditPerform: 'Audit.Perform',
    FindingManage: 'Finding.Manage',
    ActionManageOwn: 'Action.ManageOwn',
    ActionApprove: 'Action.Approve',
    ReportView: 'Report.View',
    ConfigurationManage: 'Configuration.Manage',
};
export const policyToRole = {
    CanManageAudits: EntraAppRole.AuditManage,
    CanPerformAudits: EntraAppRole.AuditPerform,
    CanManageFindings: EntraAppRole.FindingManage,
    CanManageOwnActions: EntraAppRole.ActionManageOwn,
    CanApproveActions: EntraAppRole.ActionApprove,
    CanViewReports: EntraAppRole.ReportView,
    CanManageConfiguration: EntraAppRole.ConfigurationManage,
};
