import type { FallbackProps } from 'react-error-boundary';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutlineOutlined';

/**
 * Central error handling for uncaught render/component errors (CLAUDE.md
 * "Central error handling"). Never renders raw exception content — only a
 * generic message, matching the "React must never render raw exception
 * content" rule in CLAUDE.md §9.
 */
export function AppErrorFallback({ resetErrorBoundary }: FallbackProps) {
  return (
    <Box
      role="alert"
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'center',
        gap: 2,
        minHeight: '60vh',
        px: 3,
      }}
    >
      <ErrorOutlineIcon color="error" sx={{ fontSize: 56 }} aria-hidden="true" />
      <Typography variant="h5" component="p">
        Something went wrong
      </Typography>
      <Typography color="text.secondary" sx={{ maxWidth: 480 }}>
        An unexpected error occurred while rendering this page. You can try again, or go back to the
        dashboard.
      </Typography>
      <Box sx={{ display: 'flex', gap: 2 }}>
        <Button variant="contained" onClick={resetErrorBoundary}>
          Try again
        </Button>
        <Button variant="outlined" href="/">
          Go to dashboard
        </Button>
      </Box>
    </Box>
  );
}
