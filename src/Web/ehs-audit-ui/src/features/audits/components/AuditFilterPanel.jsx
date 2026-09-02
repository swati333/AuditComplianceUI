import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import RefreshIcon from '@mui/icons-material/Refresh';
import { AUDIT_STATUSES } from '@/types/domain';
/**
 * Filters limited to what Audit.Contracts.Requests.AuditListQuery actually
 * accepts (status, location, plannedStartDateFrom/To, searchText) — the
 * Audit domain has no "audit type" or "lead auditor" field on the backend to
 * filter by, so those aren't offered here (see the phase summary for why).
 */
export function AuditFilterPanel({ filters, onChange, onRefresh, isRefreshing, }) {
    return (<Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap', alignItems: 'flex-start' }}>
      <TextField label="Search" size="small" defaultValue={filters.searchText} onChange={(event) => onChange({ searchText: event.target.value })} sx={{ minWidth: 240 }}/>
      <TextField select label="Status" size="small" value={filters.status} onChange={(event) => onChange({ status: event.target.value })} sx={{ minWidth: 180 }}>
        <MenuItem value="">All statuses</MenuItem>
        {AUDIT_STATUSES.map((option) => (<MenuItem key={option} value={option}>
            {option}
          </MenuItem>))}
      </TextField>
      <TextField label="Location" size="small" value={filters.location} onChange={(event) => onChange({ location: event.target.value })} sx={{ minWidth: 180 }}/>
      <TextField label="Planned start from" type="date" size="small" value={filters.plannedStartDateFrom} onChange={(event) => onChange({ plannedStartDateFrom: event.target.value })} slotProps={{ inputLabel: { shrink: true } }} sx={{ minWidth: 180 }}/>
      <TextField label="Planned start to" type="date" size="small" value={filters.plannedStartDateTo} onChange={(event) => onChange({ plannedStartDateTo: event.target.value })} slotProps={{ inputLabel: { shrink: true } }} sx={{ minWidth: 180 }}/>
      <Tooltip title="Refresh">
        <span>
          <IconButton aria-label="Refresh audits" onClick={onRefresh} disabled={isRefreshing} sx={{ mt: 0.5 }}>
            <RefreshIcon />
          </IconButton>
        </span>
      </Tooltip>
    </Stack>);
}
