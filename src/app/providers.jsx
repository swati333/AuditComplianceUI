import { Provider as StoreProvider } from 'react-redux';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { ErrorBoundary } from 'react-error-boundary';
import { store } from '@/app/store';
import { theme } from '@/config/theme';
import { AuthProvider } from '@/auth/AuthProvider';
import { AppErrorFallback } from '@/components/feedback/AppErrorFallback';
/** Composes every app-wide provider in one place, in the order each one depends on. */
export function AppProviders({ children }) {
    return (<ErrorBoundary FallbackComponent={AppErrorFallback}>
      <StoreProvider store={store}>
        <AuthProvider>
          <ThemeProvider theme={theme}>
            <CssBaseline />
            {children}
          </ThemeProvider>
        </AuthProvider>
      </StoreProvider>
    </ErrorBoundary>);
}
