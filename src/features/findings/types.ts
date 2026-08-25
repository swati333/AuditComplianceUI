import type { FindingSeverity, FindingStatus } from '@/types/domain';
import type { SortDirection } from '@/types/pagination';

/** Mirrors Finding.Contracts.Dtos.FindingSummaryDto. */
export interface FindingSummary {
  id: string;
  auditId: string;
  title: string;
  severity: FindingSeverity;
  status: FindingStatus;
  createdDate: string;
  modifiedDate: string | null;
}

export interface FindingComment {
  id: string;
  authorId: string;
  authorName: string;
  text: string;
  createdDate: string;
}

export interface FindingDocument {
  id: string;
  fileName: string;
  blobReference: string;
  contentType: string;
  sizeBytes: number;
  createdDate: string;
}

/** Mirrors Finding.Contracts.Dtos.FindingDetailDto. */
export interface FindingDetail {
  id: string;
  auditId: string;
  title: string;
  description: string | null;
  severity: FindingSeverity;
  status: FindingStatus;
  rootCauseAnalysis: string | null;
  rootCauseAnalysisBy: string | null;
  rootCauseAnalysisAtUtc: string | null;
  createdBy: string;
  createdDate: string;
  modifiedBy: string | null;
  modifiedDate: string | null;
  rowVersion: string;
  comments: FindingComment[];
  documents: FindingDocument[];
}

export interface FindingStatusHistoryEntry {
  id: string;
  fromStatus: FindingStatus | null;
  toStatus: FindingStatus;
  changedBy: string;
  changedAtUtc: string;
}

/** Mirrors Finding.Contracts.Requests.FindingListQuery. */
export interface FindingListQuery {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: SortDirection;
  searchText?: string;
  auditId?: string;
  status?: FindingStatus;
  severity?: FindingSeverity;
}

export interface CreateFindingRequest {
  auditId: string;
  title: string;
  description?: string;
  severity: FindingSeverity;
}

export interface UpdateFindingRequest {
  title: string;
  description?: string;
  severity: FindingSeverity;
  rowVersion: string;
}

export interface RecordRootCauseAnalysisRequest {
  text: string;
}

export interface AddCommentRequest {
  authorId: string;
  authorName: string;
  text: string;
}

/** Mirrors Finding.Contracts.Requests.AddDocumentRequest — records metadata for an already-uploaded blob (CLAUDE.md §10); this API does not itself accept a file upload. */
export interface AddDocumentRequest {
  fileName: string;
  blobReference: string;
  contentType: string;
  sizeBytes: number;
}
