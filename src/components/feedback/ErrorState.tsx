import Alert from '@mui/material/Alert';
import AlertTitle from '@mui/material/AlertTitle';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import type { ProblemDetails } from '@/types/problemDetails';
import { getErrorMessage } from '@/services/problemDetails';

export function ErrorState({
  problem,
  onRetry,
}: {
  problem: ProblemDetails;
  onRetry?: () => void;
}) {
  return (
    <Box sx={{ py: 4, px: 2 }}>
      <Alert
        severity="error"
        role="alert"
        action={onRetry && <Button onClick={onRetry}>Retry</Button>}
      >
        <AlertTitle>{problem.title ?? 'Something went wrong'}</AlertTitle>
        <Typography variant="body2">{getErrorMessage(problem)}</Typography>
        {problem.traceId && (
          <Typography variant="caption" color="text.secondary" component="div" sx={{ mt: 1 }}>
            Trace ID: {problem.traceId}
          </Typography>
        )}
      </Alert>
    </Box>
  );
}
