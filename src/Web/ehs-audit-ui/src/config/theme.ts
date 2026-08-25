import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#0F5C4C' },
    secondary: { main: '#0B5FA5' },
    error: { main: '#C0293C' },
    warning: { main: '#B5730A' },
    success: { main: '#1E7D32' },
    background: { default: '#F5F7F7', paper: '#FFFFFF' },
  },
  shape: { borderRadius: 8 },
  typography: {
    fontFamily: [
      'Inter',
      '-apple-system',
      'BlinkMacSystemFont',
      '"Segoe UI"',
      'Roboto',
      'Arial',
      'sans-serif',
    ].join(','),
  },
  components: {
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: { root: { textTransform: 'none' } },
    },
    MuiAppBar: {
      defaultProps: { elevation: 0, color: 'default' },
    },
  },
});

/** Severity/status color mapping shared across findings, action plans, and dashboards. */
export const severityColor: Record<string, 'default' | 'info' | 'warning' | 'error'> = {
  Low: 'default',
  Medium: 'info',
  High: 'warning',
  Critical: 'error',
};

export const statusColor: Record<
  string,
  'default' | 'primary' | 'info' | 'warning' | 'success' | 'error'
> = {
  Draft: 'default',
  Planned: 'info',
  InProgress: 'primary',
  Completed: 'success',
  Closed: 'default',
  Cancelled: 'error',
  Open: 'error',
  UnderReview: 'warning',
  ActionRequired: 'warning',
  Resolved: 'info',
  Verified: 'success',
  Assigned: 'info',
  SubmittedForApproval: 'warning',
  Approved: 'success',
  Rejected: 'error',
  Overdue: 'error',
};
