import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/render';
import { AuditHistoryTimeline } from '@/features/audits/components/AuditHistoryTimeline';
const entries = [
    {
        id: '1',
        fromStatus: null,
        toStatus: 'Draft',
        changedBy: 'auditor@example.com',
        changedAtUtc: '2026-08-01T00:00:00Z',
        reason: null,
    },
    {
        id: '2',
        fromStatus: 'Draft',
        toStatus: 'Planned',
        changedBy: 'auditor@example.com',
        changedAtUtc: '2026-08-02T00:00:00Z',
        reason: null,
    },
];
describe('AuditHistoryTimeline', () => {
    it('shows an empty state when there are no entries', () => {
        renderWithProviders(<AuditHistoryTimeline entries={[]}/>);
        expect(screen.getByText('No status changes recorded yet')).toBeInTheDocument();
    });
    it('renders every status transition', () => {
        renderWithProviders(<AuditHistoryTimeline entries={entries}/>);
        // "Draft" appears twice: as the first entry's toStatus and the second entry's fromStatus.
        expect(screen.getAllByText('Draft')).toHaveLength(2);
        expect(screen.getByText('Planned')).toBeInTheDocument();
        expect(screen.getAllByText(/auditor@example.com/)).toHaveLength(2);
    });
    it('renders a reason when present', () => {
        renderWithProviders(<AuditHistoryTimeline entries={[{ ...entries[1], reason: 'Rescheduled per plant manager request' }]}/>);
        expect(screen.getByText('Rescheduled per plant manager request')).toBeInTheDocument();
    });
});
