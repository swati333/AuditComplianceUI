import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { getAccessToken } from '@/auth/getAccessToken';
import { serviceBaseUrls } from '@/config/env';
import { sessionForbidden, sessionUnauthorized } from '@/app/sessionSlice';
/**
 * One fetchBaseQuery per owning microservice (CLAUDE.md §5: each service has
 * its own base URL, no shared gateway yet), every one sharing the same
 * prepareHeaders — bearer token via MSAL, plus a per-request correlation ID
 * that also becomes the request's causation ID for whatever it triggers
 * downstream (CLAUDE.md §7).
 */
const serviceQueries = Object.fromEntries(Object.entries(serviceBaseUrls).map(([service, baseUrl]) => [
    service,
    fetchBaseQuery({
        baseUrl,
        prepareHeaders: async (headers) => {
            const token = await getAccessToken();
            if (token) {
                headers.set('Authorization', `Bearer ${token}`);
            }
            headers.set('X-Correlation-Id', crypto.randomUUID());
            return headers;
        },
    }),
]));
/**
 * The one shared base query every feature's injected endpoints go through.
 * Routes each request to the right service's fetchBaseQuery by the
 * `service` field the endpoint's query() returns, and handles 401/403
 * centrally: every endpoint here represents an authenticated business call,
 * so any Unauthorized/Forbidden response is a session problem, not a
 * per-feature concern. SessionGuard (mounted once in AppShell) reacts to the
 * dispatched state by navigating — no feature code special-cases this itself.
 */
const dynamicBaseQuery = async (args, api, extraOptions) => {
    const { service, url, method, body, params } = args;
    const result = await serviceQueries[service]({ url, method, body, params }, api, extraOptions);
    if (result.error?.status === 401) {
        api.dispatch(sessionUnauthorized());
    }
    else if (result.error?.status === 403) {
        api.dispatch(sessionForbidden());
    }
    return result;
};
/**
 * The one shared RTK Query base API. Every feature injects its own endpoints
 * into this instance (services/*Api.ts files call `baseApi.injectEndpoints`)
 * instead of creating a separate `createApi` — CLAUDE.md's "one shared RTK
 * Query base API, feature-specific injected endpoints" requirement.
 */
export const baseApi = createApi({
    reducerPath: 'api',
    baseQuery: dynamicBaseQuery,
    tagTypes: [
        'Audit',
        'AuditList',
        'Checklist',
        'ChecklistList',
        'Finding',
        'FindingList',
        'Notification',
        'NotificationPreference',
    ],
    endpoints: () => ({}),
});
