import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import InboxOutlinedIcon from '@mui/icons-material/InboxOutlined';
export function EmptyState({ title, description, icon, actionLabel, onAction, }) {
    return (<Box sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            textAlign: 'center',
            gap: 1.5,
            py: 8,
            px: 3,
        }}>
      <Box sx={{ color: 'text.disabled', fontSize: 48, display: 'flex' }}>
        {icon ?? <InboxOutlinedIcon fontSize="inherit" aria-hidden="true"/>}
      </Box>
      <Typography variant="h6" component="p">
        {title}
      </Typography>
      {description && (<Typography color="text.secondary" sx={{ maxWidth: 480 }}>
          {description}
        </Typography>)}
      {actionLabel && onAction && (<Button variant="contained" onClick={onAction} sx={{ mt: 1 }}>
          {actionLabel}
        </Button>)}
    </Box>);
}
