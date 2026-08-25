import type { AuditStatus, AuditTeamRole } from '@/types/domain';
import type { SortDirection } from '@/types/pagination';

/** Mirrors Audit.Contracts.Dtos.AuditSummaryDto — the audit list projection. */
export interface AuditSummary {
  id: string;
  title: string;
  scope: string;
  location: string;
  status: AuditStatus;
  plannedStartDate: string | null;
  plannedEndDate: string | null;
  createdDate: string;
  modifiedDate: string | null;
}

export interface AuditTeamMember {
  id: string;
  userId: string;
  displayName: string;
  role: AuditTeamRole;
}

/** Mirrors Audit.Contracts.Dtos.ChecklistResponseDto exactly — field names included. */
export interface ChecklistResponse {
  id: string;
  checklistQuestionId: string;
  answerText: string;
  isCompliant: boolean | null;
  modifiedDate: string | null;
}

/** Mirrors Audit.Contracts.Dtos.ChecklistQuestionDto. No section/grouping field exists on the backend. */
export interface ChecklistQuestion {
  id: string;
  text: string;
  isMandatory: boolean;
  displayOrder: number;
}

/** Mirrors Audit.Contracts.Dtos.ChecklistDto. */
export interface Checklist {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  questions: ChecklistQuestion[];
}

/** Mirrors Audit.Contracts.Dtos.AuditDetailDto. */
export interface AuditDetail {
  id: string;
  title: string;
  description: string | null;
  scope: string;
  location: string;
  status: AuditStatus;
  checklistId: string | null;
  plannedStartDate: string | null;
  plannedEndDate: string | null;
  actualStartDate: string | null;
  actualEndDate: string | null;
  cancellationReason: string | null;
  createdBy: string;
  createdDate: string;
  modifiedBy: string | null;
  modifiedDate: string | null;
  rowVersion: string;
  teamMembers: AuditTeamMember[];
  checklistResponses: ChecklistResponse[];
}

export interface AuditStatusHistoryEntry {
  id: string;
  fromStatus: AuditStatus | null;
  toStatus: AuditStatus;
  changedBy: string;
  changedAtUtc: string;
  reason: string | null;
}

/** Mirrors Audit.Contracts.Requests.AuditListQuery. */
export interface AuditListQuery {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: SortDirection;
  searchText?: string;
  status?: AuditStatus;
  location?: string;
  plannedStartDateFrom?: string;
  plannedStartDateTo?: string;
}

export interface CreateAuditRequest {
  title: string;
  description?: string;
  scope: string;
  location: string;
}

export interface UpdateAuditRequest {
  title: string;
  description?: string;
  scope: string;
  location: string;
  rowVersion: string;
}

export interface PlanAuditRequest {
  plannedStartDate: string;
  plannedEndDate: string;
}

export interface CancelAuditRequest {
  reason?: string;
}

export interface AssignTeamMemberRequest {
  userId: string;
  displayName: string;
  role: AuditTeamRole;
}

export interface AssignChecklistRequest {
  checklistId: string;
}

/** Mirrors Audit.Contracts.Requests.RecordChecklistResponseRequest — one question per call, no batch endpoint exists. */
export interface RecordChecklistResponseRequest {
  checklistQuestionId: string;
  answerText: string;
  isCompliant: boolean | null;
}
