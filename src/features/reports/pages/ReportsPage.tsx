import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { ComingSoonState } from '@/components/feedback/ComingSoonState';

export function ReportsPage() {
  return (
    <Stack spacing={3}>
      <Typography variant="h4" component="h1">
        Reports
      </Typography>
      <ComingSoonState feature="Regulatory compliance reporting" />
    </Stack>
  );
}
