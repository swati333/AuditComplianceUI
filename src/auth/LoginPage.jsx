import { Navigate, useLocation } from 'react-router';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import ShieldOutlinedIcon from '@mui/icons-material/ShieldOutlined';
import { useAuth } from '@/auth/useAuth';
export function LoginPage() {
    const { isAuthenticated, isInitializing, login } = useAuth();
    const location = useLocation();
    const from = location.state?.from;
    if (isAuthenticated) {
        return <Navigate to={from?.pathname ?? '/'} replace/>;
    }
    return (<Box sx={{
            display: 'flex',
            minHeight: '100vh',
            alignItems: 'center',
            justifyContent: 'center',
            px: 2,
            bgcolor: 'background.default',
        }}>
      <Paper variant="outlined" sx={{ p: 5, maxWidth: 420, width: '100%', textAlign: 'center' }}>
        <ShieldOutlinedIcon color="primary" sx={{ fontSize: 48, mb: 2 }} aria-hidden="true"/>
        <Typography variant="h5" component="h1" gutterBottom>
          EHS Audit &amp; Compliance
        </Typography>
        <Typography color="text.secondary" sx={{ mb: 4 }}>
          Sign in with your organization account to plan audits, track findings and manage
          corrective actions.
        </Typography>
        <Button variant="contained" size="large" fullWidth onClick={() => login()} disabled={isInitializing}>
          Sign in
        </Button>
      </Paper>
    </Box>);
}
