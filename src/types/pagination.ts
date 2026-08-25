/** Mirrors Ehs.SharedKernel.Pagination.PagedResult<T> exactly — the shape every list endpoint returns. */
export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export type SortDirection = 'asc' | 'desc';

/** Common list-endpoint query params every feature's list query extends (CLAUDE.md §6). */
export interface PagedRequestParams {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: SortDirection;
  searchText?: string;
}

export const DEFAULT_PAGE_SIZE = 20;
