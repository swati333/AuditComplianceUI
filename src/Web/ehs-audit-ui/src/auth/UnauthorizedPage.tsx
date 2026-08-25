import { Link as RouterLink } from 'react-router';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import { UnauthorizedState } from '@/components/feedback/UnauthorizedState';

/** Route-level page for /unauthorized — reached via ProtectedRoute's policy check or a 403 from the API (SessionGuard). */
export function UnauthorizedPage() {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
      <UnauthorizedState />
      <Button variant="contained" component={RouterLink} to="/">
        Go to dashboard
      </Button>
    </Box>
  );
}
