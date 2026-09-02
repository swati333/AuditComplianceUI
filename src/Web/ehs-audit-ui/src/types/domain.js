/** Domain-wide enums shared across features, per CLAUDE.md §2 lifecycle definitions. */
export const AUDIT_STATUSES = [
    'Draft',
    'Planned',
    'InProgress',
    'Completed',
    'Closed',
    'Cancelled',
];
export const FINDING_STATUSES = [
    'Open',
    'UnderReview',
    'ActionRequired',
    'Resolved',
    'Verified',
    'Closed',
];
export const FINDING_SEVERITIES = ['Low', 'Medium', 'High', 'Critical'];
export const ACTION_PLAN_STATUSES = [
    'Assigned',
    'InProgress',
    'SubmittedForApproval',
    'Approved',
    'Closed',
    'Rejected',
    'Overdue',
    'Cancelled',
];
export const AUDIT_TEAM_ROLES = ['Auditor', 'Auditee'];
