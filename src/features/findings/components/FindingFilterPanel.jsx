import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import RefreshIcon from '@mui/icons-material/Refresh';
import { FINDING_SEVERITIES, FINDING_STATUSES, } from '@/types/domain';
/**
 * Filters limited to what Finding.Contracts.Requests.FindingListQuery
 * actually accepts (auditId, status, severity, searchText) — Finding has no
 * "responsible person" or date field on the backend to filter by, so those
 * aren't offered here (see the phase summary for why).
 */
export function FindingFilterPanel({ filters, onChange, onRefresh, isRefreshing, lockAuditId, }) {
    return (<Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap', alignItems: 'flex-start' }}>
      <TextField label="Search" size="small" defaultValue={filters.searchText} onChange={(event) => onChange({ searchText: event.target.value })} sx={{ minWidth: 240 }}/>
      {!lockAuditId && (<TextField label="Audit ID" size="small" value={filters.auditId} onChange={(event) => onChange({ auditId: event.target.value })} sx={{ minWidth: 220 }}/>)}
      <TextField select label="Severity" size="small" value={filters.severity} onChange={(event) => onChange({ severity: event.target.value })} sx={{ minWidth: 160 }}>
        <MenuItem value="">All severities</MenuItem>
        {FINDING_SEVERITIES.map((option) => (<MenuItem key={option} value={option}>
            {option}
          </MenuItem>))}
      </TextField>
      <TextField select label="Status" size="small" value={filters.status} onChange={(event) => onChange({ status: event.target.value })} sx={{ minWidth: 180 }}>
        <MenuItem value="">All statuses</MenuItem>
        {FINDING_STATUSES.map((option) => (<MenuItem key={option} value={option}>
            {option}
          </MenuItem>))}
      </TextField>
      <Tooltip title="Refresh">
        <span>
          <IconButton aria-label="Refresh findings" onClick={onRefresh} disabled={isRefreshing} sx={{ mt: 0.5 }}>
            <RefreshIcon />
          </IconButton>
        </span>
      </Tooltip>
    </Stack>);
}
