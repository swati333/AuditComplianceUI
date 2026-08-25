import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/render';
import { RequirePolicy } from '@/auth/RequirePolicy';

const mockUseAuth = vi.fn();
vi.mock('@/auth/useAuth', () => ({ useAuth: () => mockUseAuth() }));

describe('RequirePolicy', () => {
  it('renders children when the caller has the required role', () => {
    mockUseAuth.mockReturnValue({ roles: ['Audit.Manage'] });

    renderWithProviders(
      <RequirePolicy policy="CanManageAudits">
        <button type="button">Delete audit</button>
      </RequirePolicy>,
    );

    expect(screen.getByRole('button', { name: 'Delete audit' })).toBeInTheDocument();
  });

  it('renders nothing when the caller lacks the required role', () => {
    mockUseAuth.mockReturnValue({ roles: ['Finding.Manage'] });

    renderWithProviders(
      <RequirePolicy policy="CanManageAudits">
        <button type="button">Delete audit</button>
      </RequirePolicy>,
    );

    expect(screen.queryByRole('button', { name: 'Delete audit' })).not.toBeInTheDocument();
  });
});
