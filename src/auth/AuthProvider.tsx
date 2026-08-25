import type { ReactNode } from 'react';
import { MsalProvider } from '@azure/msal-react';
import { msalInstance } from '@/auth/msalInstance';

/** Central authentication provider — the only place MsalProvider is mounted. */
export function AuthProvider({ children }: { children: ReactNode }) {
  return <MsalProvider instance={msalInstance}>{children}</MsalProvider>;
}
