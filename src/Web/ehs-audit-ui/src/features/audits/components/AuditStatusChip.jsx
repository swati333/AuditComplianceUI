import { StatusChip } from '@/components/data-display/StatusChip';
/** Domain-specific alias of the shared StatusChip, scoped to Audit statuses. */
export function AuditStatusChip({ status }) {
    return <StatusChip status={status}/>;
}
