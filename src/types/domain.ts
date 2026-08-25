/** Domain-wide enums shared across features, per CLAUDE.md §2 lifecycle definitions. */

export const AUDIT_STATUSES = [
  'Draft',
  'Planned',
  'InProgress',
  'Completed',
  'Closed',
  'Cancelled',
] as const;
export type AuditStatus = (typeof AUDIT_STATUSES)[number];

export const FINDING_STATUSES = [
  'Open',
  'UnderReview',
  'ActionRequired',
  'Resolved',
  'Verified',
  'Closed',
] as const;
export type FindingStatus = (typeof FINDING_STATUSES)[number];

export const FINDING_SEVERITIES = ['Low', 'Medium', 'High', 'Critical'] as const;
export type FindingSeverity = (typeof FINDING_SEVERITIES)[number];

export const ACTION_PLAN_STATUSES = [
  'Assigned',
  'InProgress',
  'SubmittedForApproval',
  'Approved',
  'Closed',
  'Rejected',
  'Overdue',
  'Cancelled',
] as const;
export type ActionPlanStatus = (typeof ACTION_PLAN_STATUSES)[number];

export const AUDIT_TEAM_ROLES = ['Auditor', 'Auditee'] as const;
export type AuditTeamRole = (typeof AUDIT_TEAM_ROLES)[number];
