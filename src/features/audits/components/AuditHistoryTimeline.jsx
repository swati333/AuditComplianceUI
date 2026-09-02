import { StatusHistoryTimeline } from '@/components/data-display/StatusHistoryTimeline';
import { AuditStatusChip } from '@/features/audits/components/AuditStatusChip';
/** Audit-domain wrapper around the shared StatusHistoryTimeline. */
export function AuditHistoryTimeline({ entries }) {
    return (<StatusHistoryTimeline entries={entries} renderStatus={(status) => <AuditStatusChip status={status}/>}/>);
}
