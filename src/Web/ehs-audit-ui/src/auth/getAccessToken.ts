import { InteractionRequiredAuthError } from '@azure/msal-browser';
import { apiTokenRequest } from '@/auth/msalConfig';
import { msalInstance } from '@/auth/msalInstance';

/**
 * Used by baseApi's prepareHeaders on every RTK Query request. Tries a
 * silent token acquisition first; falls back to an interactive redirect only
 * when Entra genuinely requires it (consent/MFA/expired session) rather than
 * on every call, per standard MSAL guidance.
 */
export async function getAccessToken(): Promise<string | undefined> {
  const account = msalInstance.getActiveAccount();
  if (!account) {
    return undefined;
  }

  try {
    const result = await msalInstance.acquireTokenSilent({ ...apiTokenRequest, account });
    return result.accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      await msalInstance.acquireTokenRedirect(apiTokenRequest);
    }
    return undefined;
  }
}
