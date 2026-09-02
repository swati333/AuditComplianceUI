import { useNavigate, useParams, Link as RouterLink } from 'react-router';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import Chip from '@mui/material/Chip';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import FactCheckOutlinedIcon from '@mui/icons-material/FactCheckOutlined';
import HistoryOutlinedIcon from '@mui/icons-material/HistoryOutlined';
import { useGetAuditByIdQuery, useGetAuditStatusHistoryQuery } from '@/features/audits/api';
import { useGetFindingsQuery } from '@/features/findings/api';
import { AuditStatusChip } from '@/features/audits/components/AuditStatusChip';
import { AuditSummaryCard } from '@/features/audits/components/AuditSummaryCard';
import { AuditLifecycleActions } from '@/features/audits/components/AuditLifecycleActions';
import { AuditHistoryTimeline } from '@/features/audits/components/AuditHistoryTimeline';
import { DocumentUploader } from '@/features/audits/components/DocumentUploader';
import { LoadingState } from '@/components/feedback/LoadingState';
import { ErrorState } from '@/components/feedback/ErrorState';
import { RequirePolicy } from '@/auth/RequirePolicy';
import { toProblemDetails } from '@/services/problemDetails';
function FindingsSummary({ auditId }) {
    // pageSize covers the realistic case; the "N findings" count itself always comes from
    // the server's totalCount regardless, so it stays accurate even beyond this page.
    const { data, isLoading } = useGetFindingsQuery({ pageNumber: 1, pageSize: 100, auditId });
    if (isLoading)
        return <LoadingState label="Loading findings…"/>;
    const bySeverity = (data?.items ?? []).reduce((counts, finding) => {
        counts[finding.severity] = (counts[finding.severity] ?? 0) + 1;
        return counts;
    }, {});
    return (<Stack spacing={1.5}>
      <Typography>
        <strong>{data?.totalCount ?? 0}</strong> finding{data?.totalCount === 1 ? '' : 's'} recorded
        for this audit.
      </Typography>
      <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
        {Object.entries(bySeverity).map(([severity, count]) => (<Chip key={severity} label={`${severity}: ${count}`} size="small" variant="outlined"/>))}
      </Stack>
      <Button component={RouterLink} to={`/audits/${auditId}/findings`} size="small" sx={{ alignSelf: 'flex-start' }}>
        View findings for this audit
      </Button>
    </Stack>);
}
export function AuditDetailPage() {
    const { auditId = '' } = useParams();
    const navigate = useNavigate();
    const { data: audit, isLoading, isError, error, refetch } = useGetAuditByIdQuery(auditId);
    const { data: history } = useGetAuditStatusHistoryQuery(auditId);
    if (isLoading)
        return <LoadingState label="Loading audit…"/>;
    if (isError || !audit)
        return <ErrorState problem={toProblemDetails(error)} onRetry={refetch}/>;
    return (<Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h4" component="h1">
            {audit.title}
          </Typography>
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mt: 1 }}>
            <AuditStatusChip status={audit.status}/>
            <Typography color="text.secondary" variant="body2">
              {audit.location}
            </Typography>
          </Stack>
        </Box>
        <Stack direction="row" spacing={1.5}>
          <Button variant="outlined" startIcon={<FactCheckOutlinedIcon />} onClick={() => navigate(`/audits/${audit.id}/checklist`)}>
            Checklist
          </Button>
          <Button variant="outlined" startIcon={<HistoryOutlinedIcon />} onClick={() => navigate(`/audits/${audit.id}/history`)}>
            History
          </Button>
          <RequirePolicy policy="CanManageAudits">
            <Button variant="outlined" startIcon={<EditOutlinedIcon />} onClick={() => navigate(`/audits/${audit.id}/edit`)}>
              Edit
            </Button>
          </RequirePolicy>
        </Stack>
      </Stack>

      <AuditLifecycleActions audit={audit}/>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 8 }}>
          <AuditSummaryCard audit={audit}/>

          <Paper variant="outlined" sx={{ p: 3, mt: 3 }}>
            <Typography variant="h6" gutterBottom>
              Findings summary
            </Typography>
            <FindingsSummary auditId={audit.id}/>
          </Paper>

          <Paper variant="outlined" sx={{ p: 3, mt: 3 }}>
            <Typography variant="h6" gutterBottom>
              Action plans
            </Typography>
            <Typography color="text.secondary">
              Action plan tracking isn&apos;t available yet — the Action Plan service hasn&apos;t
              shipped its API.
            </Typography>
          </Paper>

          <Paper variant="outlined" sx={{ p: 3, mt: 3 }}>
            <Typography variant="h6" gutterBottom>
              Status history
            </Typography>
            <AuditHistoryTimeline entries={(history ?? []).slice(0, 3)}/>
            {history && history.length > 3 && (<Button size="small" sx={{ mt: 1 }} onClick={() => navigate(`/audits/${audit.id}/history`)}>
                View full history
              </Button>)}
          </Paper>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Team
            </Typography>
            {audit.teamMembers.length === 0 ? (<Typography color="text.secondary">No team members assigned yet.</Typography>) : (<Stack spacing={1}>
                {audit.teamMembers.map((member) => (<Stack key={member.id} direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                    <Typography variant="body2">{member.displayName}</Typography>
                    <Chip size="small" label={member.role}/>
                  </Stack>))}
              </Stack>)}
            <RequirePolicy policy="CanManageAudits">
              <Button size="small" sx={{ mt: 1.5 }} onClick={() => navigate(`/audits/${audit.id}/edit`)}>
                Manage team
              </Button>
            </RequirePolicy>
          </Paper>

          <Paper variant="outlined" sx={{ p: 3, mt: 3 }}>
            <Typography variant="h6" gutterBottom>
              Documents
            </Typography>
            <DocumentUploader documents={[]} disabledMessage="Audits don't support document attachments yet."/>
          </Paper>
        </Grid>
      </Grid>
    </Stack>);
}
