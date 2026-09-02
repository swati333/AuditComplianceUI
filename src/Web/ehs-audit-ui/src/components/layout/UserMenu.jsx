import { useState } from 'react';
import IconButton from '@mui/material/IconButton';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Avatar from '@mui/material/Avatar';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import LogoutOutlinedIcon from '@mui/icons-material/LogoutOutlined';
import { useAuth } from '@/auth/useAuth';
function initialsFor(name) {
    return name
        .split(' ')
        .filter(Boolean)
        .slice(0, 2)
        .map((part) => part[0]?.toUpperCase())
        .join('');
}
export function UserMenu() {
    const { displayName, logout } = useAuth();
    const [anchorEl, setAnchorEl] = useState(null);
    return (<>
      <IconButton onClick={(event) => setAnchorEl(event.currentTarget)} aria-label="Account menu" aria-haspopup="true" aria-expanded={Boolean(anchorEl)} size="small">
        <Avatar sx={{ width: 32, height: 32 }}>{initialsFor(displayName) || '?'}</Avatar>
      </IconButton>
      <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={() => setAnchorEl(null)}>
        <MenuItem disabled>{displayName}</MenuItem>
        <MenuItem onClick={() => logout()}>
          <ListItemIcon>
            <LogoutOutlinedIcon fontSize="small"/>
          </ListItemIcon>
          <ListItemText>Sign out</ListItemText>
        </MenuItem>
      </Menu>
    </>);
}
