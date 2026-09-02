import { StatusChip } from '@/components/data-display/StatusChip';
/** Domain-specific alias of the shared StatusChip, scoped to Finding statuses. */
export function FindingStatusChip({ status }) {
    return <StatusChip status={status}/>;
}
