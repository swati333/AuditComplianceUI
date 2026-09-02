import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link as RouterLink } from 'react-router';
import { FormProvider, useForm, useFormContext, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import ArrowBackOutlinedIcon from '@mui/icons-material/ArrowBackOutlined';
import { useGetAuditByIdQuery, useGetChecklistByIdQuery, useRecordChecklistResponseMutation, } from '@/features/audits/api';
import { checklistAnswerSchema } from '@/features/audits/schemas';
import { answerStateToIsCompliant, findResponse, isCompliantToAnswerState, } from '@/features/audits/checklistUtils';
import { ChecklistQuestion } from '@/features/audits/components/ChecklistQuestion';
import { LoadingState } from '@/components/feedback/LoadingState';
import { ErrorState } from '@/components/feedback/ErrorState';
import { EmptyState } from '@/components/feedback/EmptyState';
import { RequirePolicy } from '@/auth/RequirePolicy';
import { toProblemDetails } from '@/services/problemDetails';
const checklistFormSchema = z.object({ answers: z.record(z.string(), checklistAnswerSchema) });
export function AuditChecklistPage() {
    const { auditId = '' } = useParams();
    const navigate = useNavigate();
    const [saveError, setSaveError] = useState(null);
    const [saveSuccess, setSaveSuccess] = useState(null);
    const { data: audit, isLoading: auditLoading, isError, error, refetch, } = useGetAuditByIdQuery(auditId);
    const { data: checklist, isLoading: checklistLoading } = useGetChecklistByIdQuery(audit?.checklistId ?? '', { skip: !audit?.checklistId });
    const [recordResponse, { isLoading: isSaving }] = useRecordChecklistResponseMutation();
    const form = useForm({
        resolver: zodResolver(checklistFormSchema),
        defaultValues: { answers: {} },
    });
    useEffect(() => {
        if (!checklist || !audit)
            return;
        const answers = {};
        for (const question of checklist.questions) {
            const existing = findResponse(audit.checklistResponses, question.id);
            answers[question.id] = {
                answerState: existing ? isCompliantToAnswerState(existing.isCompliant) : undefined,
                answerText: existing?.answerText ?? '',
                isMandatory: question.isMandatory,
            };
        }
        form.reset({ answers });
    }, [checklist, audit, form]);
    if (auditLoading)
        return <LoadingState label="Loading audit…"/>;
    if (isError || !audit)
        return <ErrorState problem={toProblemDetails(error)} onRetry={refetch}/>;
    if (!audit.checklistId) {
        return (<Stack spacing={3}>
        <Typography variant="h4" component="h1">
          Checklist
        </Typography>
        <EmptyState title="No checklist assigned" description="Assign a checklist to this audit before recording responses." actionLabel="Assign a checklist" onAction={() => {
                navigate(`/audits/${auditId}/edit`);
            }}/>
      </Stack>);
    }
    if (checklistLoading || !checklist)
        return <LoadingState label="Loading checklist…"/>;
    const sortedQuestions = [...checklist.questions].sort((a, b) => a.displayOrder - b.displayOrder);
    async function persistAnswers(values) {
        setSaveError(null);
        setSaveSuccess(null);
        const toSave = sortedQuestions.filter((q) => values.answers[q.id]?.answerState !== undefined);
        const results = await Promise.allSettled(toSave.map((question) => {
            const answer = values.answers[question.id];
            return recordResponse({
                auditId,
                body: {
                    checklistQuestionId: question.id,
                    answerText: answer.answerText?.trim() ?? '',
                    isCompliant: answerStateToIsCompliant(answer.answerState),
                },
            }).unwrap();
        }));
        const failures = results.filter((r) => r.status === 'rejected');
        if (failures.length > 0) {
            setSaveError(`${failures.length} of ${toSave.length} answers failed to save. Try again.`);
        }
        else {
            setSaveSuccess('Responses saved.');
        }
    }
    async function handleSaveDraft() {
        // Draft save intentionally skips mandatory-question validation — it persists whatever is filled in so far.
        await persistAnswers(form.getValues());
    }
    async function handleSubmit(values) {
        await persistAnswers(values);
    }
    return (<Stack spacing={3}>
      <Button component={RouterLink} to={`/audits/${auditId}`} startIcon={<ArrowBackOutlinedIcon />} size="small" sx={{ alignSelf: 'flex-start' }}>
        Back to audit
      </Button>

      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h4" component="h1">
            {checklist.name}
          </Typography>
          {checklist.description && (<Typography color="text.secondary" sx={{ mt: 0.5 }}>
              {checklist.description}
            </Typography>)}
        </Box>
      </Stack>

      {saveError && (<Alert severity="error" role="alert">
          {saveError}
        </Alert>)}
      {saveSuccess && (<Alert severity="success" role="status">
          {saveSuccess}
        </Alert>)}

      <FormProvider {...form}>
        <Stack component="form" spacing={2} onSubmit={form.handleSubmit(handleSubmit)} noValidate>
          {sortedQuestions.map((question) => (<QuestionField key={question.id} questionId={question.id} question={question} disabled={isSaving}/>))}

          <RequirePolicy policy="CanPerformAudits">
            <Stack direction="row" spacing={2}>
              <Button variant="outlined" onClick={handleSaveDraft} disabled={isSaving}>
                Save draft
              </Button>
              <Button type="submit" variant="contained" disabled={isSaving}>
                Submit responses
              </Button>
            </Stack>
          </RequirePolicy>
        </Stack>
      </FormProvider>
    </Stack>);
}
/**
 * Bridges one question's slice of RHF state to the plain, controlled
 * ChecklistQuestion component (via useWatch/setValue rather than Controller,
 * so ChecklistQuestion stays reusable outside a form context too).
 */
function QuestionField({ questionId, question, disabled, }) {
    const { control, setValue, formState: { errors }, } = useFormContext();
    const answer = useWatch({ control, name: `answers.${questionId}` });
    const fieldErrors = errors.answers?.[questionId];
    return (<ChecklistQuestion question={question} answerState={answer?.answerState} answerText={answer?.answerText ?? ''} disabled={disabled} onAnswerStateChange={(state) => setValue(`answers.${questionId}.answerState`, state, { shouldDirty: true })} onAnswerTextChange={(text) => setValue(`answers.${questionId}.answerText`, text, { shouldDirty: true })} error={{
            answerState: fieldErrors?.answerState?.message,
            answerText: fieldErrors?.answerText?.message,
        }}/>);
}
