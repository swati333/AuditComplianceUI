import { useEffect } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Alert from '@mui/material/Alert';
import { findingFormSchema, type FindingFormValues } from '@/features/findings/schemas';
import {
  useCreateFindingMutation,
  useGetFindingByIdQuery,
  useUpdateFindingMutation,
} from '@/features/findings/api';
import { FindingForm } from '@/features/findings/components/FindingForm';
import { LoadingState } from '@/components/feedback/LoadingState';
import { ErrorState } from '@/components/feedback/ErrorState';
import { toProblemDetails, getErrorMessage } from '@/services/problemDetails';

export function FindingFormPage() {
  const { findingId, auditId: lockedAuditId } = useParams<{
    findingId?: string;
    auditId?: string;
  }>();
  const [searchParams] = useSearchParams();
  const isEdit = Boolean(findingId);
  const navigate = useNavigate();

  const {
    data: existing,
    isLoading,
    isError,
    error,
    refetch,
  } = useGetFindingByIdQuery(findingId!, { skip: !isEdit });
  const [createFinding, createState] = useCreateFindingMutation();
  const [updateFinding, updateState] = useUpdateFindingMutation();

  const form = useForm<FindingFormValues>({
    resolver: zodResolver(findingFormSchema),
    defaultValues: {
      auditId: lockedAuditId ?? searchParams.get('auditId') ?? '',
      title: '',
      description: '',
      severity: 'Medium',
    },
  });

  useEffect(() => {
    if (existing) {
      form.reset({
        auditId: existing.auditId,
        title: existing.title,
        description: existing.description ?? '',
        severity: existing.severity,
      });
    }
  }, [existing, form]);

  if (isEdit && isLoading) return <LoadingState label="Loading finding…" />;
  if (isEdit && isError) return <ErrorState problem={toProblemDetails(error)} onRetry={refetch} />;

  const mutationState = isEdit ? updateState : createState;

  async function onSubmit(values: FindingFormValues) {
    try {
      if (isEdit && existing) {
        const result = await updateFinding({
          id: existing.id,
          body: {
            title: values.title,
            description: values.description,
            severity: values.severity,
            rowVersion: existing.rowVersion,
          },
        }).unwrap();
        navigate(`/findings/${result.id}`);
      } else {
        const result = await createFinding(values).unwrap();
        navigate(`/findings/${result.id}`);
      }
    } catch {
      // Surfaced via mutationState.error below — RTK Query mutations reject on non-2xx responses.
    }
  }

  return (
    <Box sx={{ maxWidth: 640 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        {isEdit ? 'Edit finding' : 'New finding'}
      </Typography>

      <FormProvider {...form}>
        <Stack component="form" spacing={3} onSubmit={form.handleSubmit(onSubmit)} noValidate>
          {mutationState.isError && (
            <Alert severity="error" role="alert">
              {getErrorMessage(toProblemDetails(mutationState.error))}
            </Alert>
          )}
          <FindingForm auditIdLocked={isEdit || Boolean(lockedAuditId)} />

          <Stack direction="row" spacing={2}>
            <Button type="submit" variant="contained" disabled={mutationState.isLoading}>
              {isEdit ? 'Save changes' : 'Create finding'}
            </Button>
            <Button variant="outlined" onClick={() => navigate(-1)}>
              Cancel
            </Button>
          </Stack>
        </Stack>
      </FormProvider>
    </Box>
  );
}
