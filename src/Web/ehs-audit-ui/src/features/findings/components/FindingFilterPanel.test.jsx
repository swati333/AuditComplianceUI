import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '@/test/render';
import { FindingFilterPanel, } from '@/features/findings/components/FindingFilterPanel';
const baseFilters = { searchText: '', auditId: '', status: '', severity: '' };
describe('FindingFilterPanel', () => {
    it('reports search text changes', async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        renderWithProviders(<FindingFilterPanel filters={baseFilters} onChange={onChange} onRefresh={vi.fn()}/>);
        await user.type(screen.getByLabelText('Search'), 'exit');
        expect(onChange.mock.calls.at(-1)?.[0]).toEqual({ searchText: 'exit' });
    });
    it('reports severity selection', async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        renderWithProviders(<FindingFilterPanel filters={baseFilters} onChange={onChange} onRefresh={vi.fn()}/>);
        await user.click(screen.getByLabelText('Severity'));
        await user.click(await screen.findByRole('option', { name: 'Critical' }));
        expect(onChange).toHaveBeenCalledWith({ severity: 'Critical' });
    });
    it('hides the Audit ID field when lockAuditId is set', () => {
        renderWithProviders(<FindingFilterPanel filters={baseFilters} onChange={vi.fn()} onRefresh={vi.fn()} lockAuditId/>);
        expect(screen.queryByLabelText('Audit ID')).not.toBeInTheDocument();
    });
    it('calls onRefresh when the refresh button is clicked', async () => {
        const user = userEvent.setup();
        const onRefresh = vi.fn();
        renderWithProviders(<FindingFilterPanel filters={baseFilters} onChange={vi.fn()} onRefresh={onRefresh}/>);
        await user.click(screen.getByRole('button', { name: /refresh findings/i }));
        expect(onRefresh).toHaveBeenCalledTimes(1);
    });
});
