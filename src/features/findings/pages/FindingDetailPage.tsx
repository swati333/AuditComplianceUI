import { useParams } from 'react-router';
import { useGetFindingByIdQuery } from '@/features/findings/api';
import { FindingDetails } from '@/features/findings/components/FindingDetails';
import { LoadingState } from '@/components/feedback/LoadingState';
import { ErrorState } from '@/components/feedback/ErrorState';
import { toProblemDetails } from '@/services/problemDetails';

export function FindingDetailPage() {
  const { findingId = '' } = useParams<{ findingId: string }>();
  const { data: finding, isLoading, isError, error, refetch } = useGetFindingByIdQuery(findingId);

  if (isLoading) return <LoadingState label="Loading finding…" />;
  if (isError || !finding)
    return <ErrorState problem={toProblemDetails(error)} onRetry={refetch} />;

  return <FindingDetails finding={finding} />;
}
