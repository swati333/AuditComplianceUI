import Stack from '@mui/material/Stack';
import Button from '@mui/material/Button';
import type { FindingDetail } from '@/features/findings/types';
import {
  useCloseFindingMutation,
  useRequireFindingActionMutation,
  useResolveFindingMutation,
  useStartFindingReviewMutation,
  useVerifyFindingMutation,
} from '@/features/findings/api';
import { ErrorState } from '@/components/feedback/ErrorState';
import { RequirePolicy } from '@/auth/RequirePolicy';
import { toProblemDetails } from '@/services/problemDetails';

/**
 * Only renders the lifecycle transition valid for the finding's current
 * status, gated by CanManageFindings (the only policy any of these five
 * endpoints requires). Finding.Api's forward path is Open → UnderReview →
 * ActionRequired → Resolved → Verified → Closed — there is no "reject
 * verification" or other backward-transition endpoint, so none is offered
 * here (see the phase summary for why). The API remains the source of truth
 * for whether a transition is actually allowed.
 */
export function FindingLifecycleActions({ finding }: { finding: FindingDetail }) {
  const [startReview, startState] = useStartFindingReviewMutation();
  const [requireAction, requireState] = useRequireFindingActionMutation();
  const [resolveFinding, resolveState] = useResolveFindingMutation();
  const [verifyFinding, verifyState] = useVerifyFindingMutation();
  const [closeFinding, closeState] = useCloseFindingMutation();

  const actionError =
    startState.error ??
    requireState.error ??
    resolveState.error ??
    verifyState.error ??
    closeState.error;

  return (
    <Stack spacing={2}>
      {actionError && <ErrorState problem={toProblemDetails(actionError)} />}

      <RequirePolicy policy="CanManageFindings">
        <Stack direction="row" spacing={1.5} sx={{ flexWrap: 'wrap' }}>
          {finding.status === 'Open' && (
            <Button
              variant="contained"
              onClick={() => startReview(finding.id)}
              disabled={startState.isLoading}
            >
              Start review
            </Button>
          )}
          {finding.status === 'UnderReview' && (
            <Button
              variant="contained"
              onClick={() => requireAction(finding.id)}
              disabled={requireState.isLoading}
            >
              Mark action required
            </Button>
          )}
          {finding.status === 'ActionRequired' && (
            <Button
              variant="contained"
              onClick={() => resolveFinding(finding.id)}
              disabled={resolveState.isLoading}
            >
              Resolve
            </Button>
          )}
          {finding.status === 'Resolved' && (
            <Button
              variant="contained"
              onClick={() => verifyFinding(finding.id)}
              disabled={verifyState.isLoading}
            >
              Verify
            </Button>
          )}
          {finding.status === 'Verified' && (
            <Button
              variant="contained"
              onClick={() => closeFinding(finding.id)}
              disabled={closeState.isLoading}
            >
              Close finding
            </Button>
          )}
        </Stack>
      </RequirePolicy>
    </Stack>
  );
}
