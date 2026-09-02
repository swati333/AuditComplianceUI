import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined';
import FactCheckOutlinedIcon from '@mui/icons-material/FactCheckOutlined';
import ReportProblemOutlinedIcon from '@mui/icons-material/ReportProblemOutlined';
import AssignmentTurnedInOutlinedIcon from '@mui/icons-material/AssignmentTurnedInOutlined';
import NotificationsOutlinedIcon from '@mui/icons-material/NotificationsOutlined';
import BarChartOutlinedIcon from '@mui/icons-material/BarChartOutlined';
export const navItems = [
    { label: 'Dashboard', path: '/', icon: DashboardOutlinedIcon },
    { label: 'Audits', path: '/audits', icon: FactCheckOutlinedIcon },
    { label: 'Findings', path: '/findings', icon: ReportProblemOutlinedIcon },
    { label: 'Action Plans', path: '/action-plans', icon: AssignmentTurnedInOutlinedIcon },
    { label: 'Notifications', path: '/notifications', icon: NotificationsOutlinedIcon },
    { label: 'Reports', path: '/reports', icon: BarChartOutlinedIcon, policy: 'CanViewReports' },
];
