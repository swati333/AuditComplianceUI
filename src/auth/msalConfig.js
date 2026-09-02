import { LogLevel } from '@azure/msal-browser';
import { authConfig } from '@/config/env';
/**
 * Central MSAL configuration. Values come only from env config (CLAUDE.md
 * §10: no hard-coded client IDs/tokens) — see .env.example. PKCE + a public
 * client, so no client secret is ever needed here.
 */
export const msalConfig = {
    auth: {
        clientId: authConfig.clientId,
        authority: `https://login.microsoftonline.com/${authConfig.tenantId}`,
        redirectUri: authConfig.redirectUri,
        postLogoutRedirectUri: authConfig.redirectUri,
    },
    cache: {
        cacheLocation: 'sessionStorage',
    },
    system: {
        loggerOptions: {
            loggerCallback: (level, message, containsPii) => {
                if (containsPii)
                    return;
                if (level === LogLevel.Error)
                    console.error(message);
            },
        },
    },
};
export const loginRequest = {
    scopes: [authConfig.apiScope],
};
export const apiTokenRequest = {
    scopes: [authConfig.apiScope],
};
