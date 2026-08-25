import { StatusHistoryTimeline } from '@/components/data-display/StatusHistoryTimeline';
import { FindingStatusChip } from '@/features/findings/components/FindingStatusChip';
import type { FindingStatusHistoryEntry } from '@/features/findings/types';
import type { FindingStatus } from '@/types/domain';

/** Finding-domain wrapper around the shared StatusHistoryTimeline. */
export function FindingHistoryTimeline({ entries }: { entries: FindingStatusHistoryEntry[] }) {
  return (
    <StatusHistoryTimeline
      entries={entries}
      renderStatus={(status) => <FindingStatusChip status={status as FindingStatus} />}
    />
  );
}
