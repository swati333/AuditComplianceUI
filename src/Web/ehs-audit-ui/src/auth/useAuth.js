import { useMsal } from '@azure/msal-react';
import { useMemo } from 'react';
import { loginRequest } from '@/auth/msalConfig';
export function useAuth() {
    const { instance, accounts, inProgress } = useMsal();
    const account = accounts[0];
    const roles = useMemo(() => {
        const claims = account?.idTokenClaims;
        return (claims?.roles ?? []);
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
