import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/render';
import { StatusHistoryTimeline } from '@/components/data-display/StatusHistoryTimeline';
describe('StatusHistoryTimeline', () => {
    it('shows an empty state when there are no entries', () => {
        renderWithProviders(<StatusHistoryTimeline entries={[]} renderStatus={(s) => <span>{s}</span>}/>);
        expect(screen.getByText('No status changes recorded yet')).toBeInTheDocument();
    });
    it('delegates status rendering to renderStatus', () => {
        renderWithProviders(<StatusHistoryTimeline entries={[
                {
                    id: '1',
                    fromStatus: null,
                    toStatus: 'Open',
                    changedBy: 'x',
                    changedAtUtc: '2026-08-01T00:00:00Z',
                },
            ]} renderStatus={(status) => <span data-testid={`status-${status}`}>{status}</span>}/>);
        expect(screen.getByTestId('status-Open')).toBeInTheDocument();
    });
    it('renders a reason line only when present', () => {
        renderWithProviders(<StatusHistoryTimeline entries={[
                {
                    id: '1',
                    fromStatus: 'Draft',
                    toStatus: 'Cancelled',
                    changedBy: 'x',
                    changedAtUtc: '2026-08-01T00:00:00Z',
                    reason: 'Scope changed',
                },
            ]} renderStatus={(s) => <span>{s}</span>}/>);
        expect(screen.getByText('Scope changed')).toBeInTheDocument();
    });
});
