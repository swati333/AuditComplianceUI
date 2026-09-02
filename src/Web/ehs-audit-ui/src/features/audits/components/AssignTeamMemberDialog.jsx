import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import Alert from '@mui/material/Alert';
import { assignTeamMemberSchema } from '@/features/audits/schemas';
import { useAssignTeamMemberMutation } from '@/features/audits/api';
import { FormTextField } from '@/components/forms/FormTextField';
import { FormSelect } from '@/components/forms/FormSelect';
import { toProblemDetails, getErrorMessage } from '@/services/problemDetails';
export function AssignTeamMemberDialog({ auditId, open, onClose, }) {
    const [assignTeamMember, { isLoading, error }] = useAssignTeamMemberMutation();
    const form = useForm({
        resolver: zodResolver(assignTeamMemberSchema),
        defaultValues: { userId: '', displayName: '', role: 'Auditor' },
    });
    async function onSubmit(values) {
        try {
            await assignTeamMember({ auditId, body: values }).unwrap();
            form.reset();
            onClose();
        }
        catch {
            // Error surfaced below via the mutation's `error` state.
        }
    }
    return (<Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Assign team member</DialogTitle>
      <FormProvider {...form}>
        <Stack component="form" onSubmit={form.handleSubmit(onSubmit)} noValidate>
          <DialogContent>
            <Stack spacing={2}>
              {error && <Alert severity="error">{getErrorMessage(toProblemDetails(error))}</Alert>}
              <FormTextField name="displayName" label="Display name" required autoFocus/>
              <FormTextField name="userId" label="User ID (Entra object ID)" required/>
              <FormSelect name="role" label="Role" required options={[
            { value: 'Auditor', label: 'Auditor' },
            { value: 'Auditee', label: 'Auditee' },
        ]}/>
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={onClose}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={isLoading}>
              Assign
            </Button>
          </DialogActions>
        </Stack>
      </FormProvider>
    </Dialog>);
}
