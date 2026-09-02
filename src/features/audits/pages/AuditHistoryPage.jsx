import { useParams, Link as RouterLink } from 'react-router';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import ArrowBackOutlinedIcon from '@mui/icons-material/ArrowBackOutlined';
import { useGetAuditByIdQuery, useGetAuditStatusHistoryQuery } from '@/features/audits/api';
import { AuditHistoryTimeline } from '@/features/audits/components/AuditHistoryTimeline';
import { LoadingState } from '@/components/feedback/LoadingState';
import { ErrorState } from '@/components/feedback/ErrorState';
import { toProblemDetails } from '@/services/problemDetails';
export function AuditHistoryPage() {
    const { auditId = '' } = useParams();
    const { data: audit, isLoading: auditLoading } = useGetAuditByIdQuery(auditId);
    const { data: history, isLoading: historyLoading, isError, error, refetch, } = useGetAuditStatusHistoryQuery(auditId);
    if (auditLoading || historyLoading)
        return <LoadingState label="Loading history…"/>;
    if (isError)
        return <ErrorState problem={toProblemDetails(error)} onRetry={refetch}/>;
    return (<Stack spacing={3}>
      <Button component={RouterLink} to={`/audits/${auditId}`} startIcon={<ArrowBackOutlinedIcon />} size="small" sx={{ alignSelf: 'flex-start' }}>
        Back to audit
      </Button>
      <Typography variant="h4" component="h1">
        {audit ? `${audit.title} — history` : 'Audit history'}
      </Typography>
      <Paper variant="outlined" sx={{ p: 3 }}>
        <AuditHistoryTimeline entries={history ?? []}/>
      </Paper>
    </Stack>);
}
