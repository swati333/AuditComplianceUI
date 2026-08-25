import { describe, expect, it } from 'vitest';
import { act, screen } from '@testing-library/react';
import { useLocation } from 'react-router';
import { createTestStore, renderWithProviders } from '@/test/render';
import { SessionGuard } from '@/app/SessionGuard';
import { sessionForbidden, sessionUnauthorized } from '@/app/sessionSlice';

function LocationProbe() {
  const location = useLocation();
  return <div data-testid="location">{location.pathname}</div>;
}

describe('SessionGuard', () => {
  it('navigates to /login when the session becomes unauthorized', () => {
    const store = createTestStore();
    renderWithProviders(
      <>
        <SessionGuard />
        <LocationProbe />
      </>,
      { store, initialEntries: ['/audits'] },
    );

    expect(screen.getByTestId('location')).toHaveTextContent('/audits');

    act(() => {
      store.dispatch(sessionUnauthorized());
    });

    expect(screen.getByTestId('location')).toHaveTextContent('/login');
  });

  it('navigates to /unauthorized when the session becomes forbidden', () => {
    const store = createTestStore();
    renderWithProviders(
      <>
        <SessionGuard />
        <LocationProbe />
      </>,
      { store, initialEntries: ['/reports'] },
    );

    act(() => {
      store.dispatch(sessionForbidden());
    });

    expect(screen.getByTestId('location')).toHaveTextContent('/unauthorized');
  });

  it('leaves the location alone while the session is ok', () => {
    const store = createTestStore();
    renderWithProviders(
      <>
        <SessionGuard />
        <LocationProbe />
      </>,
      { store, initialEntries: ['/audits'] },
    );

    expect(screen.getByTestId('location')).toHaveTextContent('/audits');
  });
});
