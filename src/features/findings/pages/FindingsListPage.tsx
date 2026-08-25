import { useEffect, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import AddIcon from '@mui/icons-material/Add';
import { useGetFindingsQuery } from '@/features/findings/api';
import type { FindingSummary } from '@/features/findings/types';
import {
  FindingFilterPanel,
  type FindingFilters,
} from '@/features/findings/components/FindingFilterPanel';
import { FindingStatusChip } from '@/features/findings/components/FindingStatusChip';
import { DEFAULT_PAGE_SIZE, type SortDirection } from '@/types/pagination';
import { DataTable, type DataTableColumn } from '@/components/data-display/DataTable';
import { SeverityChip } from '@/components/data-display/SeverityChip';
import { ErrorState } from '@/components/feedback/ErrorState';
import { RequirePolicy } from '@/auth/RequirePolicy';
import { toProblemDetails } from '@/services/problemDetails';
import { formatDate } from '@/utils/formatDate';
import { useDebouncedValue } from '@/utils/useDebouncedValue';

const columns: DataTableColumn<FindingSummary>[] = [
  { id: 'title', label: 'Title', sortable: true, render: (row) => row.title },
  { id: 'severity', label: 'Severity', render: (row) => <SeverityChip severity={row.severity} /> },
  { id: 'status', label: 'Status', render: (row) => <FindingStatusChip status={row.status} /> },
  {
    id: 'createdDate',
    label: 'Created',
    sortable: true,
    render: (row) => formatDate(row.createdDate),
  },
];

function filtersFromParams(searchParams: URLSearchParams, lockedAuditId?: string): FindingFilters {
  return {
    searchText: searchParams.get('q') ?? '',
    auditId: lockedAuditId ?? searchParams.get('auditId') ?? '',
    status: (searchParams.get('status') as FindingFilters['status']) ?? '',
    severity: (searchParams.get('severity') as FindingFilters['severity']) ?? '',
  };
}

export function FindingsListPage() {
  const navigate = useNavigate();
  const { auditId: lockedAuditId } = useParams<{ auditId?: string }>();
  const [searchParams, setSearchParams] = useSearchParams();

  const pageNumber = Number(searchParams.get('page') ?? '1');
  const pageSize = Number(searchParams.get('pageSize') ?? String(DEFAULT_PAGE_SIZE));
  const sortBy = searchParams.get('sortBy') ?? 'createdDate';
  const sortDirection = (searchParams.get('sortDirection') as SortDirection | null) ?? 'desc';

  const [filters, setFilters] = useState<FindingFilters>(() =>
    filtersFromParams(searchParams, lockedAuditId),
  );
  const debouncedSearchText = useDebouncedValue(filters.searchText, 400);

  useEffect(() => {
    const params = new URLSearchParams(searchParams);
    if (debouncedSearchText) params.set('q', debouncedSearchText);
    else params.delete('q');
    params.set('page', '1');
    setSearchParams(params, { replace: true });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearchText]);

  const effectiveAuditId = lockedAuditId ?? filters.auditId;

  const { data, isFetching, isError, error, refetch } = useGetFindingsQuery({
    pageNumber,
    pageSize,
    searchText: debouncedSearchText || undefined,
    status: filters.status || undefined,
    severity: filters.severity || undefined,
    auditId: effectiveAuditId || undefined,
    sortBy,
    sortDirection,
  });

  function updateUrlParams(next: Record<string, string | undefined>) {
    const params = new URLSearchParams(searchParams);
    for (const [key, value] of Object.entries(next)) {
      if (value) params.set(key, value);
      else params.delete(key);
    }
    setSearchParams(params);
  }

  function handleFilterChange(next: Partial<FindingFilters>) {
    setFilters((prev) => ({ ...prev, ...next }));
    if (next.status !== undefined) updateUrlParams({ status: next.status || undefined, page: '1' });
    if (next.severity !== undefined)
      updateUrlParams({ severity: next.severity || undefined, page: '1' });
    if (next.auditId !== undefined)
      updateUrlParams({ auditId: next.auditId || undefined, page: '1' });
  }

  const newFindingHref = lockedAuditId
    ? `/audits/${lockedAuditId}/findings/new`
    : effectiveAuditId
      ? `/findings/new?auditId=${effectiveAuditId}`
      : '/findings/new';

  return (
    <Stack spacing={3}>
      <Stack
        direction="row"
        sx={{ justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}
      >
        <Typography variant="h4" component="h1">
          Findings
        </Typography>
        <RequirePolicy policy="CanManageFindings">
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => navigate(newFindingHref)}
          >
            New finding
          </Button>
        </RequirePolicy>
      </Stack>

      {lockedAuditId && (
        <Box>
          <Typography color="text.secondary" variant="body2">
            Showing findings for this audit only.
          </Typography>
        </Box>
      )}

      <FindingFilterPanel
        filters={filters}
        onChange={handleFilterChange}
        onRefresh={refetch}
        isRefreshing={isFetching}
        lockAuditId={Boolean(lockedAuditId)}
      />

      {isError ? (
        <ErrorState problem={toProblemDetails(error)} onRetry={refetch} />
      ) : (
        <DataTable
          caption="Findings"
          columns={columns}
          rows={data?.items ?? []}
          getRowId={(row) => row.id}
          getRowSx={(row) => (row.severity === 'Critical' ? { bgcolor: 'error.50' } : undefined)}
          totalCount={data?.totalCount ?? 0}
          pageNumber={pageNumber}
          pageSize={pageSize}
          onPageChange={(page) => updateUrlParams({ page: String(page) })}
          onPageSizeChange={(size) => updateUrlParams({ pageSize: String(size), page: '1' })}
          sortBy={sortBy}
          sortDirection={sortDirection}
          onSortChange={(column) =>
            updateUrlParams({
              sortBy: column,
              sortDirection: sortBy === column && sortDirection === 'asc' ? 'desc' : 'asc',
            })
          }
          onRowClick={(row) => navigate(`/findings/${row.id}`)}
          isLoading={isFetching}
          emptyTitle="No findings found"
          emptyDescription="Try clearing your filters, or record a new finding from an audit."
        />
      )}
    </Stack>
  );
}
