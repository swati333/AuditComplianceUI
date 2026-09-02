import { useState } from 'react';
import Stack from '@mui/material/Stack';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import { useCancelAuditMutation, useCloseAuditMutation, useCompleteAuditMutation, useStartAuditMutation, } from '@/features/audits/api';
import { PlanAuditDialog } from '@/features/audits/components/PlanAuditDialog';
import { ConfirmationDialog } from '@/components/feedback/ConfirmationDialog';
import { ErrorState } from '@/components/feedback/ErrorState';
import { RequirePolicy } from '@/auth/RequirePolicy';
import { toProblemDetails } from '@/services/problemDetails';
const NON_CANCELLABLE_STATUSES = ['Closed', 'Cancelled'];
/**
 * Only renders the lifecycle transition valid for the audit's current status
 * (CLAUDE.md: "only a Planned audit can Start", etc.) and gated by the
 * policy that action's endpoint actually requires — never both Plan and
 * Start at once, and never an action the caller's role can't perform anyway.
 * The API remains the source of truth for whether a transition is actually
 * allowed; this only avoids offering buttons that would just 409.
 */
export function AuditLifecycleActions({ audit }) {
    const [planOpen, setPlanOpen] = useState(false);
    const [cancelOpen, setCancelOpen] = useState(false);
    const [cancelReason, setCancelReason] = useState('');
    const [startAudit, startState] = useStartAuditMutation();
    const [completeAudit, completeState] = useCompleteAuditMutation();
    const [closeAudit, closeState] = useCloseAuditMutation();
    const [cancelAudit, cancelState] = useCancelAuditMutation();
    const actionError = startState.error ?? completeState.error ?? closeState.error ?? cancelState.error;
    async function confirmCancel() {
        try {
            await cancelAudit({ id: audit.id, body: { reason: cancelReason || undefined } }).unwrap();
            setCancelOpen(false);
            setCancelReason('');
        }
        catch {
            // Kept open — the error renders below via `actionError` so the user can retry.
        }
    }
    return (<Stack spacing={2}>
      {actionError && <ErrorState problem={toProblemDetails(actionError)}/>}

      <Stack direction="row" spacing={1.5} sx={{ flexWrap: 'wrap' }}>
        {audit.status === 'Draft' && (<RequirePolicy policy="CanManageAudits">
            <Button variant="contained" onClick={() => setPlanOpen(true)}>
              Plan audit
            </Button>
          </RequirePolicy>)}
        {audit.status === 'Planned' && (<RequirePolicy policy="CanPerformAudits">
            <Button variant="contained" onClick={() => startAudit(audit.id)} disabled={startState.isLoading}>
              Start audit
            </Button>
          </RequirePolicy>)}
        {audit.status === 'InProgress' && (<RequirePolicy policy="CanPerformAudits">
            <Button variant="contained" onClick={() => completeAudit(audit.id)} disabled={completeState.isLoading}>
              Complete audit
            </Button>
          </RequirePolicy>)}
        {audit.status === 'Completed' && (<RequirePolicy policy="CanManageAudits">
            <Button variant="contained" onClick={() => closeAudit(audit.id)} disabled={closeState.isLoading}>
              Close audit
            </Button>
          </RequirePolicy>)}
        {!NON_CANCELLABLE_STATUSES.includes(audit.status) && (<RequirePolicy policy="CanManageAudits">
            <Button color="error" variant="outlined" onClick={() => setCancelOpen(true)}>
              Cancel audit
            </Button>
          </RequirePolicy>)}
      </Stack>

      <PlanAuditDialog auditId={audit.id} open={planOpen} onClose={() => setPlanOpen(false)}/>

      <ConfirmationDialog open={cancelOpen} title="Cancel this audit?" message="This will cancel the audit. This action cannot be undone." confirmLabel="Cancel audit" destructive isConfirming={cancelState.isLoading} onConfirm={confirmCancel} onCancel={() => setCancelOpen(false)}>
        <TextField label="Reason (optional)" value={cancelReason} onChange={(event) => setCancelReason(event.target.value)} multiline minRows={2} fullWidth sx={{ mt: 2 }} slotProps={{ htmlInput: { maxLength: 1000 } }}/>
      </ConfirmationDialog>
    </Stack>);
}
