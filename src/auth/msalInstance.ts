import { EventType, PublicClientApplication } from '@azure/msal-browser';
import { msalConfig } from '@/auth/msalConfig';

export const msalInstance = new PublicClientApplication(msalConfig);

// Keep MSAL's "active account" in sync so acquireTokenSilent (used by
// baseApi on every request) always has an account to target, without every
// call site needing to pick one out of a multi-account cache.
msalInstance.addEventCallback((event) => {
  if (
    (event.eventType === EventType.LOGIN_SUCCESS ||
      event.eventType === EventType.ACQUIRE_TOKEN_SUCCESS) &&
    event.payload &&
    'account' in event.payload &&
    event.payload.account
  ) {
    msalInstance.setActiveAccount(event.payload.account);
  }
});

let initializePromise: Promise<void> | null = null;

/** Idempotent — safe to call from multiple entry points (main.tsx, tests). */
export function ensureMsalInitialized(): Promise<void> {
  initializePromise ??= msalInstance.initialize().then(async () => {
    await msalInstance.handleRedirectPromise();
    const accounts = msalInstance.getAllAccounts();
    if (!msalInstance.getActiveAccount() && accounts.length > 0) {
      msalInstance.setActiveAccount(accounts[0]);
    }
  });
  return initializePromise;
}
