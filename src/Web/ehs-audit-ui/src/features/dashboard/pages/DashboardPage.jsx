import { Link as RouterLink } from 'react-router';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Grid from '@mui/material/Grid';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import Box from '@mui/material/Box';
import { Bar, BarChart, CartesianGrid, Cell, ResponsiveContainer, Tooltip, XAxis, YAxis, } from 'recharts';
import { useGetAuditsQuery } from '@/features/audits/api';
import { useGetFindingsQuery } from '@/features/findings/api';
import { useFindingSeverityBreakdown } from '@/features/dashboard/useFindingSeverityBreakdown';
import { StatusChip } from '@/components/data-display/StatusChip';
import { LoadingState } from '@/components/feedback/LoadingState';
import { theme } from '@/config/theme';
import { formatDate } from '@/utils/formatDate';
function KpiCard({ label, value }) {
    return (<Paper variant="outlined" sx={{ p: 2.5 }}>
      <Typography color="text.secondary" variant="body2">
        {label}
      </Typography>
      <Typography variant="h4" component="p" sx={{ mt: 0.5 }}>
        {value}
      </Typography>
    </Paper>);
}
const SEVERITY_HEX = {
    Low: theme.palette.grey[500],
    Medium: theme.palette.info.main,
    High: theme.palette.warning.main,
    Critical: theme.palette.error.main,
};
export function DashboardPage() {
    const { data: recentAudits, isLoading: auditsLoading } = useGetAuditsQuery({
        pageNumber: 1,
        pageSize: 5,
        sortBy: 'createdDate',
        sortDirection: 'desc',
    });
    const { data: openFindings, isLoading: findingsLoading } = useGetFindingsQuery({
        pageNumber: 1,
        pageSize: 1,
    });
    const { data: criticalFindings } = useGetFindingsQuery({
        pageNumber: 1,
        pageSize: 1,
        severity: 'Critical',
    });
    const { data: severityBreakdown, isLoading: breakdownLoading } = useFindingSeverityBreakdown();
    if (auditsLoading || findingsLoading) {
        return <LoadingState label="Loading dashboard…"/>;
    }
    return (<Stack spacing={3}>
      <Typography variant="h4" component="h1">
        Dashboard
      </Typography>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 4 }}>
          <KpiCard label="Total audits" value={recentAudits?.totalCount ?? 0}/>
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <KpiCard label="Total findings" value={openFindings?.totalCount ?? 0}/>
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <KpiCard label="Critical findings" value={criticalFindings?.totalCount ?? 0}/>
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Findings by severity
            </Typography>
            {breakdownLoading ? (<LoadingState label="Loading chart…"/>) : (<Box sx={{ height: 280 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={severityBreakdown} margin={{ left: 0, right: 16, top: 8, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false}/>
                    <XAxis dataKey="severity"/>
                    <YAxis allowDecimals={false}/>
                    <Tooltip />
                    <Bar dataKey="count" radius={[4, 4, 0, 0]}>
                      {severityBreakdown.map((entry) => (<Cell key={entry.severity} fill={SEVERITY_HEX[entry.severity]}/>))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </Box>)}
          </Paper>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Recent audits
            </Typography>
            {recentAudits && recentAudits.items.length > 0 ? (<List disablePadding>
                {recentAudits.items.map((audit) => (<ListItemButton key={audit.id} component={RouterLink} to={`/audits/${audit.id}`} divider>
                    <ListItemText primary={audit.title} secondary={`${audit.location} · ${formatDate(audit.createdDate)}`}/>
                    <StatusChip status={audit.status}/>
                  </ListItemButton>))}
              </List>) : (<Typography color="text.secondary">No audits yet.</Typography>)}
          </Paper>
        </Grid>
      </Grid>
    </Stack>);
}
