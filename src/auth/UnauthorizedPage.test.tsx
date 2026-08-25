import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/render';
import { UnauthorizedPage } from '@/auth/UnauthorizedPage';

describe('UnauthorizedPage', () => {
  it('renders the access-restricted message and a link back to the dashboard', () => {
    renderWithProviders(<UnauthorizedPage />);

    expect(screen.getByRole('alert')).toHaveTextContent('Access restricted');
    expect(screen.getByRole('link', { name: /go to dashboard/i })).toHaveAttribute('href', '/');
  });
});
