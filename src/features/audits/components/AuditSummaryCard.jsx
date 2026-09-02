import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { formatDate } from '@/utils/formatDate';
function Field({ label, value }) {
    return (<Box>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography>{value}</Typography>
    </Box>);
}
/** Summary/dates/location card for an audit's detail page — read-only display of AuditDetailDto's fields. */
export function AuditSummaryCard({ audit }) {
    return (<Paper variant="outlined" sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Details
      </Typography>
      <Stack spacing={1.5}>
        <Field label="Scope" value={audit.scope}/>
        {audit.description && <Field label="Description" value={audit.description}/>}
        <Field label="Location" value={audit.location}/>
        <Stack direction="row" spacing={4} sx={{ flexWrap: 'wrap' }}>
          <Field label="Planned start" value={formatDate(audit.plannedStartDate)}/>
          <Field label="Planned end" value={formatDate(audit.plannedEndDate)}/>
          <Field label="Actual start" value={formatDate(audit.actualStartDate)}/>
          <Field label="Actual end" value={formatDate(audit.actualEndDate)}/>
        </Stack>
        {audit.cancellationReason && (<Field label="Cancellation reason" value={audit.cancellationReason}/>)}
      </Stack>
    </Paper>);
}
