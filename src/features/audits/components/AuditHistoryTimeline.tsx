import { StatusHistoryTimeline } from '@/components/data-display/StatusHistoryTimeline';
import { AuditStatusChip } from '@/features/audits/components/AuditStatusChip';
import type { AuditStatusHistoryEntry } from '@/features/audits/types';
import type { AuditStatus } from '@/types/domain';

/** Audit-domain wrapper around the shared StatusHistoryTimeline. */
export function AuditHistoryTimeline({ entries }: { entries: AuditStatusHistoryEntry[] }) {
  return (
    <StatusHistoryTimeline
      entries={entries}
      renderStatus={(status) => <AuditStatusChip status={status as AuditStatus} />}
    />
  );
}
