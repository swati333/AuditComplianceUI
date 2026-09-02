import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import FormControlLabel from '@mui/material/FormControlLabel';
import Switch from '@mui/material/Switch';
import { useAuth } from '@/auth/useAuth';
import { useGetNotificationPreferencesQuery, useSetNotificationPreferenceMutation, } from '@/features/notifications/api';
import { LoadingState } from '@/components/feedback/LoadingState';
import { ErrorState } from '@/components/feedback/ErrorState';
import { toProblemDetails } from '@/services/problemDetails';
const CHANNELS = [
    { value: 'Email', label: 'Email' },
    { value: 'InApp', label: 'In-app' },
    { value: 'Sms', label: 'SMS' },
];
export function NotificationPreferencesPage() {
    const { account } = useAuth();
    const userId = account?.localAccountId ?? '';
    const { data: preferences, isLoading, isError, error, refetch, } = useGetNotificationPreferencesQuery(userId, {
        skip: !userId,
    });
    const [setPreference] = useSetNotificationPreferenceMutation();
    if (isLoading)
        return <LoadingState label="Loading preferences…"/>;
    if (isError)
        return <ErrorState problem={toProblemDetails(error)} onRetry={refetch}/>;
    function isEnabled(channel) {
        return preferences?.find((p) => p.channel === channel)?.isEnabled ?? true;
    }
    return (<Stack spacing={3} sx={{ maxWidth: 480 }}>
      <Typography variant="h4" component="h1">
        Notification preferences
      </Typography>
      <Paper variant="outlined" sx={{ p: 3 }}>
        <Stack spacing={1}>
          {CHANNELS.map((channel) => (<FormControlLabel key={channel.value} control={<Switch checked={isEnabled(channel.value)} onChange={(event) => setPreference({
                    userId,
                    channel: channel.value,
                    isEnabled: event.target.checked,
                })}/>} label={channel.label}/>))}
        </Stack>
      </Paper>
    </Stack>);
}
