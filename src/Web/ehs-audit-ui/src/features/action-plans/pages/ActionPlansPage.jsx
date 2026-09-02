import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { ComingSoonState } from '@/components/feedback/ComingSoonState';
export function ActionPlansPage() {
    return (<Stack spacing={3}>
      <Typography variant="h4" component="h1">
        Action Plans
      </Typography>
      <ComingSoonState feature="Action plan tracking"/>
    </Stack>);
}
