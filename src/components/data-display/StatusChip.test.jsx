import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/render';
import { StatusChip } from '@/components/data-display/StatusChip';
describe('StatusChip', () => {
    it('renders a space-separated label for a PascalCase status', () => {
        renderWithProviders(<StatusChip status="InProgress"/>);
        expect(screen.getByText('In Progress')).toBeInTheDocument();
    });
    it('renders single-word statuses unchanged', () => {
        renderWithProviders(<StatusChip status="Closed"/>);
        expect(screen.getByText('Closed')).toBeInTheDocument();
    });
});
