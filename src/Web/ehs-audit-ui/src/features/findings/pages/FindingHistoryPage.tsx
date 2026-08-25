import { useParams, Link as RouterLink } from 'react-router';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import ArrowBackOutlinedIcon from '@mui/icons-material/ArrowBackOutlined';
import { useGetFindingByIdQuery, useGetFindingStatusHistoryQuery } from '@/features/findings/api';
import { FindingHistoryTimeline } from '@/features/findings/components/FindingHistoryTimeline';
import { LoadingState } from '@/components/feedback/LoadingState';
import { ErrorState } from '@/components/feedback/ErrorState';
import { toProblemDetails } from '@/services/problemDetails';

export function FindingHistoryPage() {
  const { findingId = '' } = useParams<{ findingId: string }>();

  const { data: finding, isLoading: findingLoading } = useGetFindingByIdQuery(findingId);
  const {
    data: history,
    isLoading: historyLoading,
    isError,
    error,
    refetch,
  } = useGetFindingStatusHistoryQuery(findingId);

  if (findingLoading || historyLoading) return <LoadingState label="Loading history…" />;
  if (isError) return <ErrorState problem={toProblemDetails(error)} onRetry={refetch} />;

  return (
    <Stack spacing={3}>
      <Button
        component={RouterLink}
        to={`/findings/${findingId}`}
        startIcon={<ArrowBackOutlinedIcon />}
        size="small"
        sx={{ alignSelf: 'flex-start' }}
      >
        Back to finding
      </Button>
      <Typography variant="h4" component="h1">
        {finding ? `${finding.title} — history` : 'Finding history'}
      </Typography>
      <Paper variant="outlined" sx={{ p: 3 }}>
        <FindingHistoryTimeline entries={history ?? []} />
      </Paper>
    </Stack>
  );
}
