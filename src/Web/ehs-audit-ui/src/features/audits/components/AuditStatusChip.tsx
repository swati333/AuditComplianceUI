import { StatusChip } from '@/components/data-display/StatusChip';
import type { AuditStatus } from '@/types/domain';

/** Domain-specific alias of the shared StatusChip, scoped to Audit statuses. */
export function AuditStatusChip({ status }: { status: AuditStatus }) {
  return <StatusChip status={status} />;
}
