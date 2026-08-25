import { StatusChip } from '@/components/data-display/StatusChip';
import type { FindingStatus } from '@/types/domain';

/** Domain-specific alias of the shared StatusChip, scoped to Finding statuses. */
export function FindingStatusChip({ status }: { status: FindingStatus }) {
  return <StatusChip status={status} />;
}
