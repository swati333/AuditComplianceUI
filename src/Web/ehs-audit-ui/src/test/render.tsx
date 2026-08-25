import { useState, type ReactElement, type ReactNode } from 'react';
import { render, type RenderOptions } from '@testing-library/react';
import { Provider as StoreProvider } from 'react-redux';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { createMemoryRouter, RouterProvider } from 'react-router';
import { configureStore, type Store } from '@reduxjs/toolkit';
import { baseApi } from '@/services/baseApi';
import { sessionReducer } from '@/app/sessionSlice';
import { theme } from '@/config/theme';

/**
 * A fresh store per test (not the app's singleton `store`) so RTK Query
 * cache/tags never leak between tests. auth/useAuth should be mocked
 * per-test with vi.mock — this wrapper deliberately omits MsalProvider so
 * tests don't depend on real MSAL initialization. Mirrors app/store.ts's
 * reducer shape so any component under test can use useAppSelector safely.
 */
function createTestStore() {
  return configureStore({
    reducer: { [baseApi.reducerPath]: baseApi.reducer, session: sessionReducer },
    middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(baseApi.middleware),
  });
}

function makeAllProviders(initialEntries: string[], store: Store) {
  return function AllProviders({ children }: { children: ReactNode }) {
    // A real data router (not plain MemoryRouter) — useBlocker/useBeforeUnload
    // (unsaved-changes warnings) only work inside a data router's context.
    // A single catch-all route renders `children` regardless of the current
    // path; tests that need :param matching wrap `children` in their own
    // <Routes>/<Route>, which still works nested inside a data router leaf.
    const [router] = useState(() =>
      createMemoryRouter([{ path: '*', element: children }], { initialEntries }),
    );

    return (
      <StoreProvider store={store}>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <RouterProvider router={router} />
        </ThemeProvider>
      </StoreProvider>
    );
  };
}

export function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'> & { initialEntries?: string[]; store?: Store },
) {
  const { initialEntries = ['/'], store = createTestStore(), ...renderOptions } = options ?? {};
  return render(ui, { wrapper: makeAllProviders(initialEntries, store), ...renderOptions });
}

export { createTestStore };

export * from '@testing-library/react';
