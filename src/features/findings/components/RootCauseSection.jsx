import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { rootCauseAnalysisSchema, } from '@/features/findings/schemas';
import { useRecordRootCauseAnalysisMutation } from '@/features/findings/api';
import { FormTextField } from '@/components/forms/FormTextField';
import { ErrorState } from '@/components/feedback/ErrorState';
import { RequirePolicy } from '@/auth/RequirePolicy';
import { toProblemDetails } from '@/services/problemDetails';
import { formatDateTime } from '@/utils/formatDate';
/** Root cause analysis: read-only once recorded (RecordRootCauseAnalysisRequest has no "edit" endpoint — it's write-once via this one POST). */
export function RootCauseSection({ finding }) {
    const [recordRootCause, { isLoading, error }] = useRecordRootCauseAnalysisMutation();
    const form = useForm({
        resolver: zodResolver(rootCauseAnalysisSchema),
        defaultValues: { text: '' },
    });
    async function onSubmit(values) {
        try {
            await recordRootCause({ id: finding.id, body: values }).unwrap();
            form.reset();
        }
        catch {
            // Surfaced via `error` below.
        }
    }
    return (<Paper variant="outlined" sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Root cause analysis
      </Typography>
      {finding.rootCauseAnalysis ? (<Box>
          <Typography>{finding.rootCauseAnalysis}</Typography>
          <Typography variant="caption" color="text.secondary">
            {finding.rootCauseAnalysisBy} · {formatDateTime(finding.rootCauseAnalysisAtUtc)}
          </Typography>
        </Box>) : (<RequirePolicy policy="CanManageFindings">
          <FormProvider {...form}>
            {error && <ErrorState problem={toProblemDetails(error)}/>}
            <Stack component="form" spacing={1.5} onSubmit={form.handleSubmit(onSubmit)} noValidate>
              <FormTextField name="text" label="Root cause analysis" multiline minRows={3}/>
              <Button type="submit" variant="contained" disabled={isLoading} sx={{ alignSelf: 'flex-start' }}>
                Save analysis
              </Button>
            </Stack>
          </FormProvider>
        </RequirePolicy>)}
    </Paper>);
}
