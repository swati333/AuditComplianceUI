import type { ComponentType } from 'react';
import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined';
import FactCheckOutlinedIcon from '@mui/icons-material/FactCheckOutlined';
import ReportProblemOutlinedIcon from '@mui/icons-material/ReportProblemOutlined';
import AssignmentTurnedInOutlinedIcon from '@mui/icons-material/AssignmentTurnedInOutlined';
import NotificationsOutlinedIcon from '@mui/icons-material/NotificationsOutlined';
import BarChartOutlinedIcon from '@mui/icons-material/BarChartOutlined';
import type { PolicyName } from '@/config/policies';

export interface NavItem {
  label: string;
  path: string;
  icon: ComponentType<{ fontSize?: 'small' | 'medium' | 'large' }>;
  /** Reads are authenticated-only for most features (CLAUDE.md §6 controllers); only gate nav on a policy where the read endpoint itself requires one. */
  policy?: PolicyName;
}

export const navItems: NavItem[] = [
  { label: 'Dashboard', path: '/', icon: DashboardOutlinedIcon },
  { label: 'Audits', path: '/audits', icon: FactCheckOutlinedIcon },
  { label: 'Findings', path: '/findings', icon: ReportProblemOutlinedIcon },
  { label: 'Action Plans', path: '/action-plans', icon: AssignmentTurnedInOutlinedIcon },
  { label: 'Notifications', path: '/notifications', icon: NotificationsOutlinedIcon },
  { label: 'Reports', path: '/reports', icon: BarChartOutlinedIcon, policy: 'CanViewReports' },
];
