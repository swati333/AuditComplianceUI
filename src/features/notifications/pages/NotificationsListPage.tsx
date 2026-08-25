import { useState } from 'react';
import { Link as RouterLink } from 'react-router';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import Chip from '@mui/material/Chip';
import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import TablePagination from '@mui/material/TablePagination';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import { useAuth } from '@/auth/useAuth';
import {
  useGetNotificationsQuery,
  useMarkNotificationReadMutation,
} from '@/features/notifications/api';
import { LoadingState } from '@/components/feedback/LoadingState';
import { ErrorState } from '@/components/feedback/ErrorState';
import { EmptyState } from '@/components/feedback/EmptyState';
import { toProblemDetails } from '@/services/problemDetails';
import { formatDateTime } from '@/utils/formatDate';
import { DEFAULT_PAGE_SIZE } from '@/types/pagination';

export function NotificationsListPage() {
  const { account } = useAuth();
  const userId = account?.localAccountId;
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const { data, isLoading, isError, error, refetch } = useGetNotificationsQuery(
    { pageNumber, pageSize, recipientUserId: userId, sortDirection: 'desc' },
    { skip: !userId },
  );
  const [markRead] = useMarkNotificationReadMutation();

  return (
    <Stack spacing={3}>
      <Stack
        direction="row"
        sx={{ justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}
      >
        <Typography variant="h4" component="h1">
          Notifications
        </Typography>
        <Button
          component={RouterLink}
          to="/notifications/preferences"
          startIcon={<SettingsOutlinedIcon />}
          variant="outlined"
        >
          Preferences
        </Button>
      </Stack>

      {isLoading && <LoadingState label="Loading notifications…" />}
      {isError && <ErrorState problem={toProblemDetails(error)} onRetry={refetch} />}
      {!isLoading && !isError && (data?.items.length ?? 0) === 0 && (
        <EmptyState title="No notifications" description="You're all caught up." />
      )}

      {!isLoading && !isError && (data?.items.length ?? 0) > 0 && (
        <Paper variant="outlined">
          <List disablePadding>
            {data!.items.map((notification) => (
              <ListItemButton
                key={notification.id}
                onClick={() => !notification.isRead && markRead(notification.id)}
                divider
                sx={{ opacity: notification.isRead ? 0.7 : 1 }}
              >
                <ListItemText
                  primary={
                    <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                      <Typography sx={{ fontWeight: notification.isRead ? 400 : 600 }}>
                        {notification.subject}
                      </Typography>
                      {!notification.isRead && <Chip size="small" color="primary" label="New" />}
                    </Stack>
                  }
                  secondary={
                    <>
                      <Box component="span" sx={{ display: 'block' }}>
                        {notification.body}
                      </Box>
                      <Box
                        component="span"
                        sx={{ display: 'block', typography: 'caption', color: 'text.secondary' }}
                      >
                        {formatDateTime(notification.createdDate)} · {notification.channel}
                      </Box>
                    </>
                  }
                />
              </ListItemButton>
            ))}
          </List>
          <TablePagination
            component="div"
            count={data?.totalCount ?? 0}
            page={pageNumber - 1}
            rowsPerPage={pageSize}
            onPageChange={(_event, newPage) => setPageNumber(newPage + 1)}
            onRowsPerPageChange={(event) => {
              setPageSize(Number(event.target.value));
              setPageNumber(1);
            }}
            rowsPerPageOptions={[10, 20, 50]}
          />
        </Paper>
      )}
    </Stack>
  );
}
