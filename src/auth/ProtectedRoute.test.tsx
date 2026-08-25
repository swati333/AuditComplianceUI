import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/render';
import { ProtectedRoute } from '@/auth/ProtectedRoute';

const mockUseAuth = vi.fn();
vi.mock('@/auth/useAuth', () => ({ useAuth: () => mockUseAuth() }));

describe('ProtectedRoute', () => {
  it('shows a loading state while MSAL is still initializing', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false, isInitializing: true, roles: [] });

    renderWithProviders(
      <ProtectedRoute>
        <div>Secret content</div>
      </ProtectedRoute>,
    );

    expect(screen.getByRole('status')).toBeInTheDocument();
    expect(screen.queryByText('Secret content')).not.toBeInTheDocument();
  });

  it('renders an unauthorized state when the caller lacks the required policy', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true, isInitializing: false, roles: [] });

    renderWithProviders(
      <ProtectedRoute policy="CanManageAudits">
        <div>Secret content</div>
      </ProtectedRoute>,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Access restricted');
    expect(screen.queryByText('Secret content')).not.toBeInTheDocument();
  });

  it('renders children once authenticated and authorized', () => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isInitializing: false,
      roles: ['Audit.Manage'],
    });

    renderWithProviders(
      <ProtectedRoute policy="CanManageAudits">
        <div>Secret content</div>
      </ProtectedRoute>,
    );

    expect(screen.getByText('Secret content')).toBeInTheDocument();
  });
});
