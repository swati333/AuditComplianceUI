import { Link as RouterLink } from 'react-router';
import IconButton from '@mui/material/IconButton';
import Badge from '@mui/material/Badge';
import NotificationsOutlinedIcon from '@mui/icons-material/NotificationsOutlined';
import { useAuth } from '@/auth/useAuth';
import { useGetNotificationsQuery } from '@/features/notifications/api';
export function NotificationBell() {
    const { account } = useAuth();
    const userId = account?.localAccountId;
    const { data } = useGetNotificationsQuery({ pageNumber: 1, pageSize: 50, recipientUserId: userId }, { skip: !userId, pollingInterval: 60_000 });
    const unreadCount = data?.items.filter((n) => !n.isRead).length ?? 0;
    return (<IconButton component={RouterLink} to="/notifications" aria-label={`Notifications, ${unreadCount} unread`} size="large">
      <Badge badgeContent={unreadCount} color="error" max={99}>
        <NotificationsOutlinedIcon />
      </Badge>
    </IconButton>);
}
