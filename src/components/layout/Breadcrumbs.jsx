import { Fragment } from 'react';
import { Link as RouterLink, useLocation } from 'react-router';
import MuiBreadcrumbs from '@mui/material/Breadcrumbs';
import Link from '@mui/material/Link';
import Typography from '@mui/material/Typography';
import HomeOutlinedIcon from '@mui/icons-material/HomeOutlined';
import { navItems } from '@/components/layout/navItems';
const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const topLevelLabels = new Map(navItems.map((item) => [item.path.replace('/', ''), item.label]));
function labelFor(segment, index) {
    if (index === 0 && topLevelLabels.has(segment)) {
        return topLevelLabels.get(segment);
    }
    if (segment === 'new')
        return 'New';
    if (segment === 'edit')
        return 'Edit';
    if (segment === 'preferences')
        return 'Preferences';
    if (segment === 'unauthorized')
        return 'Unauthorized';
    if (GUID_PATTERN.test(segment)) {
        return 'Details';
    }
    return segment.charAt(0).toUpperCase() + segment.slice(1);
}
/** Derives the breadcrumb trail from the current URL — no per-page configuration needed. */
export function Breadcrumbs() {
    const location = useLocation();
    const segments = location.pathname.split('/').filter(Boolean);
    if (segments.length === 0) {
        return null;
    }
    return (<MuiBreadcrumbs aria-label="Breadcrumb" sx={{ mb: 2 }}>
      <Link component={RouterLink} to="/" underline="hover" color="inherit" sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <HomeOutlinedIcon fontSize="inherit" aria-hidden="true"/>
        Dashboard
      </Link>
      {segments.map((segment, index) => {
            const path = `/${segments.slice(0, index + 1).join('/')}`;
            const isLast = index === segments.length - 1;
            const label = labelFor(segment, index);
            return (<Fragment key={path}>
            {isLast ? (<Typography color="text.primary">{label}</Typography>) : (<Link component={RouterLink} to={path} underline="hover" color="inherit">
                {label}
              </Link>)}
          </Fragment>);
        })}
    </MuiBreadcrumbs>);
}
