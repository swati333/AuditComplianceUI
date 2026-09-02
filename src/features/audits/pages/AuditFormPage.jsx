import { useEffect, useState } from 'react';
import { Link as RouterLink, useNavigate, useParams } from 'react-router';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Alert from '@mui/material/Alert';
import Paper from '@mui/material/Paper';
import Link from '@mui/material/Link';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Chip from '@mui/material/Chip';
import IconButton from '@mui/material/IconButton';
import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import PersonAddOutlinedIcon from '@mui/icons-material/PersonAddOutlined';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlineOutlined';
import { auditFormSchema } from '@/features/audits/schemas';
import { useAssignChecklistMutation, useCreateAuditMutation, useGetAuditByIdQuery, useGetChecklistsQuery, useRemoveTeamMemberMutation, useUpdateAuditMutation, } from '@/features/audits/api';
import { AuditForm } from '@/features/audits/components/AuditForm';
import { AssignTeamMemberDialog } from '@/features/audits/components/AssignTeamMemberDialog';
import { LoadingState } from '@/components/feedback/LoadingState';
import { ErrorState } from '@/components/feedback/ErrorState';
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog';
import { toProblemDetails, getErrorMessage } from '@/services/problemDetails';
import { formatDate } from '@/utils/formatDate';
import { useUnsavedChangesWarning } from '@/utils/useUnsavedChangesWarning';
/** Team management is a live action (its own endpoints), not part of the submitted form — edit mode only, since an audit needs an id first. */
function TeamSection({ audit }) {
    const [assignOpen, setAssignOpen] = useState(false);
    const [removingMemberId, setRemovingMemberId] = useState(null);
    const [removeTeamMember, { isLoading: isRemoving }] = useRemoveTeamMemberMutation();
    async function confirmRemove() {
        if (!removingMemberId)
            return;
        try {
            await removeTeamMember({ auditId: audit.id, teamMemberId: removingMemberId }).unwrap();
        }
        catch {
            // Error is not fatal to the page — the member simply stays listed; RTK Query already rolled back the optimistic removal.
        }
        finally {
            setRemovingMemberId(null);
        }
    }
    return (<Paper variant="outlined" sx={{ p: 3 }}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
        <Typography variant="h6">Audit team</Typography>
        <IconButton size="small" aria-label="Assign team member" onClick={() => setAssignOpen(true)}>
          <PersonAddOutlinedIcon fontSize="small"/>
        </IconButton>
      </Stack>
      {audit.teamMembers.length === 0 ? (<Typography color="text.secondary">No team members assigned yet.</Typography>) : (<List dense>
          {audit.teamMembers.map((member) => (<ListItem key={member.id} disableGutters secondaryAction={<IconButton edge="end" size="small" aria-label={`Remove ${member.displayName}`} onClick={() => setRemovingMemberId(member.id)}>
                  <DeleteOutlineIcon fontSize="small"/>
                </IconButton>}>
              <ListItemText primary={member.displayName} secondary={<Chip size="small" label={member.role} sx={{ mt: 0.5 }}/>}/>
            </ListItem>))}
        </List>)}
      <AssignTeamMemberDialog auditId={audit.id} open={assignOpen} onClose={() => setAssignOpen(false)}/>
      <ConfirmationDialog open={removingMemberId !== null} title="Remove team member?" message="Remove this member from the audit team?" confirmLabel="Remove" destructive isConfirming={isRemoving} onConfirm={confirmRemove} onCancel={() => setRemovingMemberId(null)}/>
    </Paper>);
}
/** Checklist assignment is likewise a live action (POST /audits/{id}/checklist), not a form field — edit mode only. */
function ChecklistSection({ audit }) {
    const { data: checklists, isLoading } = useGetChecklistsQuery();
    const [assignChecklist, { isLoading: isAssigning, error }] = useAssignChecklistMutation();
    const [selected, setSelected] = useState(audit.checklistId ?? '');
    const currentChecklist = checklists?.find((c) => c.id === audit.checklistId);
    async function handleAssign() {
        if (!selected)
            return;
        try {
            await assignChecklist({ auditId: audit.id, body: { checklistId: selected } }).unwrap();
        }
        catch {
            // Surfaced via `error` below.
        }
    }
    return (<Paper variant="outlined" sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Assigned checklist
      </Typography>
      {error && <ErrorState problem={toProblemDetails(error)}/>}
      {currentChecklist ? (<Typography sx={{ mb: 2 }}>
          Currently assigned: <strong>{currentChecklist.name}</strong>
        </Typography>) : (<Typography color="text.secondary" sx={{ mb: 2 }}>
          No checklist assigned yet.
        </Typography>)}
      <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
        <TextField select label="Checklist" size="small" value={selected} onChange={(event) => setSelected(event.target.value)} disabled={isLoading} sx={{ minWidth: 240 }}>
          {(checklists ?? []).map((checklist) => (<MenuItem key={checklist.id} value={checklist.id}>
              {checklist.name}
            </MenuItem>))}
        </TextField>
        <Button variant="outlined" onClick={handleAssign} disabled={!selected || isAssigning}>
          Assign
        </Button>
      </Stack>
    </Paper>);
}
export function AuditFormPage() {
    const { auditId } = useParams();
    const isEdit = Boolean(auditId);
    const navigate = useNavigate();
    const { data: existing, isLoading, isError, error, refetch, } = useGetAuditByIdQuery(auditId, { skip: !isEdit });
    const [createAudit, createState] = useCreateAuditMutation();
    const [updateAudit, updateState] = useUpdateAuditMutation();
    const form = useForm({
        resolver: zodResolver(auditFormSchema),
        defaultValues: { title: '', description: '', scope: '', location: '' },
    });
    const { formState: { isDirty, isSubmitSuccessful }, } = form;
    const blocker = useUnsavedChangesWarning(isDirty && !isSubmitSuccessful);
    useEffect(() => {
        if (existing) {
            form.reset({
                title: existing.title,
                description: existing.description ?? '',
                scope: existing.scope,
                location: existing.location,
            });
        }
    }, [existing, form]);
    if (isEdit && isLoading)
        return <LoadingState label="Loading audit…"/>;
    if (isEdit && isError)
        return <ErrorState problem={toProblemDetails(error)} onRetry={refetch}/>;
    const mutationState = isEdit ? updateState : createState;
    async function onSubmit(values) {
        try {
            if (isEdit && existing) {
                const result = await updateAudit({
                    id: existing.id,
                    body: { ...values, rowVersion: existing.rowVersion },
                }).unwrap();
                navigate(`/audits/${result.id}`);
            }
            else {
                const result = await createAudit(values).unwrap();
                navigate(`/audits/${result.id}`);
            }
        }
        catch {
            // Surfaced via mutationState.error below — RTK Query mutations reject on non-2xx responses.
        }
    }
    return (<Stack spacing={3} sx={{ maxWidth: 720 }}>
      <Typography variant="h4" component="h1">
        {isEdit ? 'Edit audit' : 'New audit'}
      </Typography>

      <FormProvider {...form}>
        <Stack component="form" spacing={3} onSubmit={form.handleSubmit(onSubmit)} noValidate>
          {mutationState.isError && (<Alert severity="error" role="alert">
              {getErrorMessage(toProblemDetails(mutationState.error))}
            </Alert>)}
          <AuditForm />

          <Stack direction="row" spacing={2}>
            <Button type="submit" variant="contained" disabled={mutationState.isLoading}>
              {isEdit ? 'Save changes' : 'Create audit'}
            </Button>
            <Button variant="outlined" onClick={() => navigate(-1)}>
              Cancel
            </Button>
          </Stack>
        </Stack>
      </FormProvider>

      {isEdit && existing && (<>
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Planned dates
            </Typography>
            <Typography color="text.secondary">
              {existing.plannedStartDate
                ? `${formatDate(existing.plannedStartDate)} – ${formatDate(existing.plannedEndDate)}`
                : 'Not planned yet.'}{' '}
              Set via the{' '}
              <Link component={RouterLink} to={`/audits/${existing.id}`}>
                audit detail page
              </Link>
              's Plan action.
            </Typography>
          </Paper>
          <TeamSection audit={existing}/>
          <ChecklistSection audit={existing}/>
        </>)}

      <ConfirmationDialog open={blocker.state === 'blocked'} title="Discard unsaved changes?" message="You have unsaved changes to this audit. Leave without saving?" confirmLabel="Leave without saving" destructive onConfirm={() => blocker.state === 'blocked' && blocker.proceed()} onCancel={() => blocker.state === 'blocked' && blocker.reset()}/>
    </Stack>);
}
