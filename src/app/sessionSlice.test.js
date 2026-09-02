import { describe, expect, it } from 'vitest';
import { sessionAcknowledged, sessionForbidden, sessionReducer, sessionUnauthorized, } from '@/app/sessionSlice';
describe('sessionSlice', () => {
    it('starts in the ok state', () => {
        expect(sessionReducer(undefined, { type: '@@INIT' })).toEqual({ status: 'ok' });
    });
    it('moves to unauthorized on sessionUnauthorized', () => {
        const state = sessionReducer({ status: 'ok' }, sessionUnauthorized());
        expect(state.status).toBe('unauthorized');
    });
    it('moves to forbidden on sessionForbidden', () => {
        const state = sessionReducer({ status: 'ok' }, sessionForbidden());
        expect(state.status).toBe('forbidden');
    });
    it('resets to ok on sessionAcknowledged', () => {
        const state = sessionReducer({ status: 'forbidden' }, sessionAcknowledged());
        expect(state.status).toBe('ok');
    });
});
