import { useGetFindingsQuery } from '@/features/findings/api';
import { FINDING_SEVERITIES, type FindingSeverity } from '@/types/domain';

/**
 * The Finding list endpoint has no dedicated aggregation route, so this
 * composes the severity breakdown from four real, cheap (pageSize: 1) calls
 * to the existing endpoint — each response's `totalCount` is the count for
 * that severity, no items payload needed.
 */
export function useFindingSeverityBreakdown() {
  const queries = FINDING_SEVERITIES.map((severity) =>
    // eslint-disable-next-line react-hooks/rules-of-hooks -- FINDING_SEVERITIES is a fixed compile-time constant, so this list's length/order never changes across renders.
    useGetFindingsQuery({ pageNumber: 1, pageSize: 1, severity }),
  );

  const isLoading = queries.some((q) => q.isLoading);

  const data: { severity: FindingSeverity; count: number }[] = FINDING_SEVERITIES.map(
    (severity, index) => ({
      severity,
      count: queries[index].data?.totalCount ?? 0,
    }),
  );

  return { data, isLoading };
}
