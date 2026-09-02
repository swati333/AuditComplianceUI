import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
export function LoadingState({ label = 'Loading…' }) {
    return (<Box role="status" aria-live="polite" sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 2,
            py: 8,
        }}>
      <CircularProgress aria-hidden="true"/>
      <Typography color="text.secondary">{label}</Typography>
    </Box>);
}
