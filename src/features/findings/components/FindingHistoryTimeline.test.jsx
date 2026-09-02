import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/render';
import { FindingHistoryTimeline } from '@/features/findings/components/FindingHistoryTimeline';
describe('FindingHistoryTimeline', () => {
    it('shows an empty state when there are no entries', () => {
        renderWithProviders(<FindingHistoryTimeline entries={[]}/>);
        expect(screen.getByText('No status changes recorded yet')).toBeInTheDocument();
    });
    it('renders a status transition', () => {
        const entries = [
            {
                id: '1',
                fromStatus: null,
                toStatus: 'Open',
                changedBy: 'auditor@example.com',
                changedAtUtc: '2026-08-01T00:00:00Z',
            },
            {
                id: '2',
                fromStatus: 'Open',
                toStatus: 'UnderReview',
                changedBy: 'auditor@example.com',
                changedAtUtc: '2026-08-02T00:00:00Z',
            },
        ];
        renderWithProviders(<FindingHistoryTimeline entries={entries}/>);
        expect(screen.getAllByText('Open')).toHaveLength(2);
        expect(screen.getByText('Under Review')).toBeInTheDocument();
    });
});
