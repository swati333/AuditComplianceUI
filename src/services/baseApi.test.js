import { describe, expect, it } from 'vitest';
import { configureStore } from '@reduxjs/toolkit';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/server';
import { baseApi } from '@/services/baseApi';
import { sessionReducer } from '@/app/sessionSlice';
import { serviceBaseUrls } from '@/config/env';
function makeStore() {
    return configureStore({
        reducer: { [baseApi.reducerPath]: baseApi.reducer, session: sessionReducer },
        middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(baseApi.middleware),
    });
}
const probeApi = baseApi.injectEndpoints({
    endpoints: (builder) => ({
        probe: builder.query({
            query: () => ({ service: 'audit', url: '/probe' }),
        }),
    }),
    overrideExisting: true,
});
describe('baseApi central 401/403 handling', () => {
    it('dispatches sessionUnauthorized when a request comes back 401', async () => {
        server.use(http.get(`${serviceBaseUrls.audit}/probe`, () => new HttpResponse(null, { status: 401 })));
        const store = makeStore();
        await store.dispatch(probeApi.endpoints.probe.initiate());
        expect(store.getState().session.status).toBe('unauthorized');
    });
    it('dispatches sessionForbidden when a request comes back 403', async () => {
        server.use(http.get(`${serviceBaseUrls.audit}/probe`, () => new HttpResponse(null, { status: 403 })));
        const store = makeStore();
        await store.dispatch(probeApi.endpoints.probe.initiate());
        expect(store.getState().session.status).toBe('forbidden');
    });
    it('leaves the session state untouched on a successful request', async () => {
        server.use(http.get(`${serviceBaseUrls.audit}/probe`, () => HttpResponse.json({ ok: true })));
        const store = makeStore();
        await store.dispatch(probeApi.endpoints.probe.initiate());
        expect(store.getState().session.status).toBe('ok');
    });
});
