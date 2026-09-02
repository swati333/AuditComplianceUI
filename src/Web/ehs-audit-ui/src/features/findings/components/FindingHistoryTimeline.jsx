import { StatusHistoryTimeline } from '@/components/data-display/StatusHistoryTimeline';
import { FindingStatusChip } from '@/features/findings/components/FindingStatusChip';
/** Finding-domain wrapper around the shared StatusHistoryTimeline. */
export function FindingHistoryTimeline({ entries }) {
    return (<StatusHistoryTimeline entries={entries} renderStatus={(status) => <FindingStatusChip status={status}/>}/>);
}
