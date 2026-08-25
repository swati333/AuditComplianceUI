import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import Alert from '@mui/material/Alert';
import { planAuditSchema, type PlanAuditFormValues } from '@/features/audits/schemas';
import { usePlanAuditMutation } from '@/features/audits/api';
import { FormDateField } from '@/components/forms/FormDateField';
import { toProblemDetails, getErrorMessage } from '@/services/problemDetails';

export function PlanAuditDialog({
  auditId,
  open,
  onClose,
}: {
  auditId: string;
  open: boolean;
  onClose: () => void;
}) {
  const [planAudit, { isLoading, error }] = usePlanAuditMutation();
  const form = useForm<PlanAuditFormValues>({
    resolver: zodResolver(planAuditSchema),
    defaultValues: { plannedStartDate: '', plannedEndDate: '' },
  });

  async function onSubmit(values: PlanAuditFormValues) {
    try {
      await planAudit({ id: auditId, body: values }).unwrap();
      form.reset();
      onClose();
    } catch {
      // Error surfaced below via the mutation's `error` state.
    }
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Plan audit</DialogTitle>
      <FormProvider {...form}>
        <Stack component="form" onSubmit={form.handleSubmit(onSubmit)} noValidate>
          <DialogContent>
            <Stack spacing={2}>
              {error && <Alert severity="error">{getErrorMessage(toProblemDetails(error))}</Alert>}
              <FormDateField<PlanAuditFormValues>
                name="plannedStartDate"
                label="Planned start date"
                required
              />
              <FormDateField<PlanAuditFormValues>
                name="plannedEndDate"
                label="Planned end date"
                required
              />
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={onClose}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={isLoading}>
              Save plan
            </Button>
          </DialogActions>
        </Stack>
      </FormProvider>
    </Dialog>
  );
}
