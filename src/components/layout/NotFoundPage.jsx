import { Link as RouterLink } from 'react-router';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import SearchOffOutlinedIcon from '@mui/icons-material/SearchOffOutlined';
export function NotFoundPage() {
    return (<Box sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            textAlign: 'center',
            gap: 2,
            minHeight: '60vh',
            px: 3,
        }}>
      <SearchOffOutlinedIcon color="disabled" sx={{ fontSize: 56 }} aria-hidden="true"/>
      <Typography variant="h5" component="h1">
        Page not found
      </Typography>
      <Typography color="text.secondary">
        The page you're looking for doesn't exist or has moved.
      </Typography>
      <Button variant="contained" component={RouterLink} to="/">
        Go to dashboard
      </Button>
    </Box>);
}
