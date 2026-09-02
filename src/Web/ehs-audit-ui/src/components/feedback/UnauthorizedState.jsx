import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
export function UnauthorizedState({ message = 'You do not have permission to view this page. Contact your compliance administrator if you believe this is an error.', }) {
    return (<Box role="alert" sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            textAlign: 'center',
            gap: 1.5,
            py: 8,
            px: 3,
        }}>
      <LockOutlinedIcon color="disabled" sx={{ fontSize: 48 }} aria-hidden="true"/>
      <Typography variant="h6" component="p">
        Access restricted
      </Typography>
      <Typography color="text.secondary" sx={{ maxWidth: 480 }}>
        {message}
      </Typography>
    </Box>);
}
