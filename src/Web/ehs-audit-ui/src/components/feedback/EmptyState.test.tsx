import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '@/test/render';
import { EmptyState } from '@/components/feedback/EmptyState';

describe('EmptyState', () => {
  it('renders the title and description', () => {
    renderWithProviders(
      <EmptyState title="No audits found" description="Try clearing your filters." />,
    );

    expect(screen.getByText('No audits found')).toBeInTheDocument();
    expect(screen.getByText('Try clearing your filters.')).toBeInTheDocument();
  });

  it('invokes onAction when the action button is clicked', async () => {
    const user = userEvent.setup();
    const onAction = vi.fn();
    renderWithProviders(
      <EmptyState title="No audits" actionLabel="Create audit" onAction={onAction} />,
    );

    await user.click(screen.getByRole('button', { name: 'Create audit' }));

    expect(onAction).toHaveBeenCalledTimes(1);
  });

  it('does not render an action button when none is provided', () => {
    renderWithProviders(<EmptyState title="No audits" />);

    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });
});
