import type { ReactNode } from 'react';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { EmptyState } from '@/components/feedback/EmptyState';
import { formatDateTime } from '@/utils/formatDate';

export interface StatusHistoryEntryLike {
  id: string;
  fromStatus: string | null;
  toStatus: string;
  changedBy: string;
  changedAtUtc: string;
  reason?: string | null;
}

export interface StatusHistoryTimelineProps<T extends StatusHistoryEntryLike> {
  entries: T[];
  renderStatus: (status: string) => ReactNode;
}

/**
 * Generic vertical status-transition timeline shared by every domain that
 * has a status-history endpoint (Audit, Finding, …) — built from plain MUI
 * primitives (no @mui/lab Timeline, which isn't in the approved dependency
 * list). Domain-specific wrappers (AuditHistoryTimeline, FindingHistoryTimeline)
 * just supply how to render a status value as a chip.
 */
export function StatusHistoryTimeline<T extends StatusHistoryEntryLike>({
  entries,
  renderStatus,
}: StatusHistoryTimelineProps<T>) {
  if (entries.length === 0) {
    return <EmptyState title="No status changes recorded yet" />;
  }

  return (
    <Stack spacing={0}>
      {entries.map((entry, index) => {
        const isLast = index === entries.length - 1;
        return (
          <Stack key={entry.id} direction="row" spacing={2}>
            <Stack sx={{ alignItems: 'center', width: 12 }}>
              <Box
                sx={{
                  width: 12,
                  height: 12,
                  borderRadius: '50%',
                  bgcolor: 'primary.main',
                  flexShrink: 0,
                  mt: 0.75,
                }}
              />
              {!isLast && <Box sx={{ width: 2, flexGrow: 1, bgcolor: 'divider', minHeight: 24 }} />}
            </Stack>
            <Box sx={{ pb: isLast ? 0 : 2.5 }}>
              <Stack direction="row" spacing={1} sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
                {entry.fromStatus && renderStatus(entry.fromStatus)}
                {entry.fromStatus && (
                  <Typography color="text.secondary" variant="body2">
                    →
                  </Typography>
                )}
                {renderStatus(entry.toStatus)}
              </Stack>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                {entry.changedBy} · {formatDateTime(entry.changedAtUtc)}
              </Typography>
              {entry.reason && (
                <Typography variant="body2" sx={{ mt: 0.5 }}>
                  {entry.reason}
                </Typography>
              )}
            </Box>
          </Stack>
        );
      })}
    </Stack>
  );
}
