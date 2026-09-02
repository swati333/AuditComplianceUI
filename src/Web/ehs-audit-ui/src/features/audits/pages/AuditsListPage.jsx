import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import AddIcon from '@mui/icons-material/Add';
import { useGetAuditsQuery } from '@/features/audits/api';
import { AuditFilterPanel } from '@/features/audits/components/AuditFilterPanel';
import { AuditStatusChip } from '@/features/audits/components/AuditStatusChip';
import { DEFAULT_PAGE_SIZE } from '@/types/pagination';
import { DataTable } from '@/components/data-display/DataTable';
import { ErrorState } from '@/components/feedback/ErrorState';
import { RequirePolicy } from '@/auth/RequirePolicy';
import { toProblemDetails } from '@/services/problemDetails';
import { formatDate } from '@/utils/formatDate';
import { useDebouncedValue } from '@/utils/useDebouncedValue';
const columns = [
    { id: 'title', label: 'Title', sortable: true, render: (row) => row.title },
    { id: 'location', label: 'Location', sortable: true, render: (row) => row.location },
    { id: 'status', label: 'Status', render: (row) => <AuditStatusChip status={row.status}/> },
    {
        id: 'plannedstartdate',
        label: 'Planned start',
        sortable: true,
        render: (row) => formatDate(row.plannedStartDate),
    },
    {
        id: 'createdDate',
        label: 'Created',
        sortable: true,
        render: (row) => formatDate(row.createdDate),
    },
];
function filtersFromParams(searchParams) {
    return {
        searchText: searchParams.get('q') ?? '',
        status: searchParams.get('status') ?? '',
        location: searchParams.get('location') ?? '',
        plannedStartDateFrom: searchParams.get('startFrom') ?? '',
        plannedStartDateTo: searchParams.get('startTo') ?? '',
    };
}
export function AuditsListPage() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const pageNumber = Number(searchParams.get('page') ?? '1');
    const pageSize = Number(searchParams.get('pageSize') ?? String(DEFAULT_PAGE_SIZE));
    const sortBy = searchParams.get('sortBy') ?? 'createdDate';
    const sortDirection = searchParams.get('sortDirection') ?? 'desc';
    // Local, immediately-responsive filter state; the text fields (search, location)
    // are debounced before they reach the URL/query so typing doesn't fire a
    // request per keystroke.
    const [filters, setFilters] = useState(() => filtersFromParams(searchParams));
    const debouncedSearchText = useDebouncedValue(filters.searchText, 400);
    const debouncedLocation = useDebouncedValue(filters.location, 400);
    useEffect(() => {
        const params = new URLSearchParams(searchParams);
        if (debouncedSearchText)
            params.set('q', debouncedSearchText);
        else
            params.delete('q');
        if (debouncedLocation)
            params.set('location', debouncedLocation);
        else
            params.delete('location');
        params.set('page', '1');
        setSearchParams(params, { replace: true });
        // Only re-run when the debounced text values change — not on every searchParams change.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [debouncedSearchText, debouncedLocation]);
    const { data, isFetching, isError, error, refetch } = useGetAuditsQuery({
        pageNumber,
        pageSize,
        searchText: debouncedSearchText || undefined,
        status: filters.status || undefined,
        location: debouncedLocation || undefined,
        plannedStartDateFrom: filters.plannedStartDateFrom || undefined,
        plannedStartDateTo: filters.plannedStartDateTo || undefined,
        sortBy,
        sortDirection,
    });
    function updateUrlParams(next) {
        const params = new URLSearchParams(searchParams);
        for (const [key, value] of Object.entries(next)) {
            if (value)
                params.set(key, value);
            else
                params.delete(key);
        }
        setSearchParams(params);
    }
    function handleFilterChange(next) {
        setFilters((prev) => ({ ...prev, ...next }));
        // Non-debounced filters (status, dates) apply to the URL/query immediately.
        if (next.status !== undefined)
            updateUrlParams({ status: next.status || undefined, page: '1' });
        if (next.plannedStartDateFrom !== undefined) {
            updateUrlParams({ startFrom: next.plannedStartDateFrom || undefined, page: '1' });
        }
        if (next.plannedStartDateTo !== undefined) {
            updateUrlParams({ startTo: next.plannedStartDateTo || undefined, page: '1' });
        }
    }
    return (<Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Typography variant="h4" component="h1">
          Audits
        </Typography>
        <RequirePolicy policy="CanManageAudits">
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => navigate('/audits/new')}>
            New audit
          </Button>
        </RequirePolicy>
      </Stack>

      <AuditFilterPanel filters={filters} onChange={handleFilterChange} onRefresh={refetch} isRefreshing={isFetching}/>

      {isError ? (<ErrorState problem={toProblemDetails(error)} onRetry={refetch}/>) : (<Box>
          <DataTable caption="Audits" columns={columns} rows={data?.items ?? []} getRowId={(row) => row.id} totalCount={data?.totalCount ?? 0} pageNumber={pageNumber} pageSize={pageSize} onPageChange={(page) => updateUrlParams({ page: String(page) })} onPageSizeChange={(size) => updateUrlParams({ pageSize: String(size), page: '1' })} sortBy={sortBy} sortDirection={sortDirection} onSortChange={(column) => updateUrlParams({
                sortBy: column,
                sortDirection: sortBy === column && sortDirection === 'asc' ? 'desc' : 'asc',
            })} onRowClick={(row) => navigate(`/audits/${row.id}`)} isLoading={isFetching} emptyTitle="No audits found" emptyDescription="Try clearing your filters, or create a new audit to get started."/>
        </Box>)}
    </Stack>);
}
