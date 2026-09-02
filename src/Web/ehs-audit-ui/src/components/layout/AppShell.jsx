import { useState } from 'react';
import { NavLink, Outlet, useLocation } from 'react-router';
import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Drawer from '@mui/material/Drawer';
import IconButton from '@mui/material/IconButton';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import MenuIcon from '@mui/icons-material/Menu';
import Divider from '@mui/material/Divider';
import { navItems } from '@/components/layout/navItems';
import { NotificationBell } from '@/components/layout/NotificationBell';
import { UserMenu } from '@/components/layout/UserMenu';
import { Breadcrumbs } from '@/components/layout/Breadcrumbs';
import { RequirePolicy } from '@/auth/RequirePolicy';
import { SessionGuard } from '@/app/SessionGuard';
const DRAWER_WIDTH = 260;
function NavList({ onNavigate }) {
    const location = useLocation();
    return (<List component="nav" aria-label="Main navigation">
      {navItems.map((item) => {
            const content = (<ListItemButton key={item.path} component={NavLink} to={item.path} end={item.path === '/'} selected={location.pathname === item.path} onClick={onNavigate}>
            <ListItemIcon>
              <item.icon fontSize="small"/>
            </ListItemIcon>
            <ListItemText primary={item.label}/>
          </ListItemButton>);
            return item.policy ? (<RequirePolicy key={item.path} policy={item.policy}>
            {content}
          </RequirePolicy>) : (content);
        })}
    </List>);
}
export function AppShell() {
    const [mobileOpen, setMobileOpen] = useState(false);
    return (<Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <SessionGuard />
      <AppBar position="fixed" sx={{ width: { sm: `calc(100% - ${DRAWER_WIDTH}px)` }, ml: { sm: `${DRAWER_WIDTH}px` } }}>
        <Toolbar sx={{ gap: 1 }}>
          <IconButton color="inherit" aria-label="Open navigation menu" edge="start" onClick={() => setMobileOpen(true)} sx={{ display: { sm: 'none' } }}>
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" component="h1" noWrap sx={{ flexGrow: 1 }}>
            EHS Audit &amp; Compliance
          </Typography>
          <NotificationBell />
          <UserMenu />
        </Toolbar>
      </AppBar>

      <Box component="nav" sx={{ width: { sm: DRAWER_WIDTH }, flexShrink: { sm: 0 } }}>
        <Drawer variant="temporary" open={mobileOpen} onClose={() => setMobileOpen(false)} ModalProps={{ keepMounted: true }} sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': { width: DRAWER_WIDTH },
        }}>
          <Toolbar />
          <Divider />
          <NavList onNavigate={() => setMobileOpen(false)}/>
        </Drawer>
        <Drawer variant="permanent" sx={{
            display: { xs: 'none', sm: 'block' },
            '& .MuiDrawer-paper': { width: DRAWER_WIDTH, boxSizing: 'border-box' },
        }} open>
          <Toolbar />
          <Divider />
          <NavList />
        </Drawer>
      </Box>

      <Box component="main" sx={{ flexGrow: 1, width: { sm: `calc(100% - ${DRAWER_WIDTH}px)` }, minWidth: 0 }}>
        <Toolbar />
        <Box sx={{ p: { xs: 2, md: 3 } }}>
          <Breadcrumbs />
          <Outlet />
        </Box>
      </Box>
    </Box>);
}
