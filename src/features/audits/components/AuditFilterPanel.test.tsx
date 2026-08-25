import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '@/test/render';
import { AuditFilterPanel, type AuditFilters } from '@/features/audits/components/AuditFilterPanel';

const baseFilters: AuditFilters = {
  searchText: '',
  status: '',
  location: '',
  plannedStartDateFrom: '',
  plannedStartDateTo: '',
};

describe('AuditFilterPanel', () => {
  it('reports search text changes', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    renderWithProviders(
      <AuditFilterPanel filters={baseFilters} onChange={onChange} onRefresh={vi.fn()} />,
    );

    await user.type(screen.getByLabelText('Search'), 'fire');

    expect(onChange).toHaveBeenCalled();
    expect(onChange.mock.calls.at(-1)?.[0]).toEqual({ searchText: 'fire' });
  });

  it('reports status selection', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    renderWithProviders(
      <AuditFilterPanel filters={baseFilters} onChange={onChange} onRefresh={vi.fn()} />,
    );

    await user.click(screen.getByLabelText('Status'));
    await user.click(await screen.findByRole('option', { name: 'Planned' }));

    expect(onChange).toHaveBeenCalledWith({ status: 'Planned' });
  });

  it('calls onRefresh when the refresh button is clicked', async () => {
    const user = userEvent.setup();
    const onRefresh = vi.fn();

    renderWithProviders(
      <AuditFilterPanel filters={baseFilters} onChange={vi.fn()} onRefresh={onRefresh} />,
    );

    await user.click(screen.getByRole('button', { name: /refresh audits/i }));

    expect(onRefresh).toHaveBeenCalledTimes(1);
  });

  it('disables refresh while isRefreshing is true', () => {
    renderWithProviders(
      <AuditFilterPanel
        filters={baseFilters}
        onChange={vi.fn()}
        onRefresh={vi.fn()}
        isRefreshing
      />,
    );

    expect(screen.getByRole('button', { name: /refresh audits/i })).toBeDisabled();
  });
});
