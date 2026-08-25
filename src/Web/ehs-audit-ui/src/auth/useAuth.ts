import { useMsal } from '@azure/msal-react';
import { useMemo } from 'react';
import { loginRequest } from '@/auth/msalConfig';
import type { EntraAppRoleValue } from '@/config/policies';

export function useAuth() {
  const { instance, accounts, inProgress } = useMsal();
  const account = accounts[0];

  const roles = useMemo<EntraAppRoleValue[]>(() => {
    const claims = account?.idTokenClaims as { roles?: string[] } | undefined;
    return (claims?.roles ?? []) as EntraAppRoleValue[];
  }, [account]);

  return {
    isAuthenticated: Boolean(account),
    isInitializing: inProgress !== 'none',
    account,
    displayName: account?.name ?? account?.username ?? '',
    roles,
    login: () => instance.loginRedirect(loginRequest),
    logout: () => instance.logoutRedirect({ postLogoutRedirectUri: '/' }),
  };
}
