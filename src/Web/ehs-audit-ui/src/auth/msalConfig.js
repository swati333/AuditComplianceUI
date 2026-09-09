import { LogLevel } from '@azure/msal-browser';
import { authConfig } from '@/config/env';
/**
 * Central MSAL configuration. Values come only from env config (CLAUDE.md
 * §10: no hard-coded client IDs/tokens) — see .env.example. PKCE + a public
 * client, so no client secret is ever needed here.
 */

const authorityHost =
  `${authConfig.tenantSubdomain}.ciamlogin.com`;
// CIAM tenants issue OIDC metadata with the tenant ID (not the vanity
// subdomain) as the ciamlogin.com host, e.g.
// https://<tenantId>.ciamlogin.com/<tenantId>/v2.0 — MSAL only recognizes
// this as a valid issuer if that host is also listed as a known authority.
const tenantIdAuthorityHost = `${authConfig.tenantId}.ciamlogin.com`;

export const msalConfig = {
    auth: {
        clientId: authConfig.clientId,
        authority: `https://${authorityHost}/`,
        knownAuthorities: [authorityHost, tenantIdAuthorityHost],
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
