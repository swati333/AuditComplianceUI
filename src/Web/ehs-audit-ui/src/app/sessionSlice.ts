import { createSlice } from '@reduxjs/toolkit';

export type SessionStatus = 'ok' | 'unauthorized' | 'forbidden';

interface SessionState {
  status: SessionStatus;
}

const initialState: SessionState = { status: 'ok' };

/**
 * Central 401/403 handling (per this phase's auth requirements): baseApi
 * dispatches into this slice whenever any request comes back Unauthorized or
 * Forbidden, and SessionGuard (mounted once in AppShell) reacts by
 * navigating — so no feature code has to special-case auth failures itself.
 */
const sessionSlice = createSlice({
  name: 'session',
  initialState,
  reducers: {
    sessionUnauthorized: (state) => {
      state.status = 'unauthorized';
    },
    sessionForbidden: (state) => {
      state.status = 'forbidden';
    },
    sessionAcknowledged: (state) => {
      state.status = 'ok';
    },
  },
});

export const { sessionUnauthorized, sessionForbidden, sessionAcknowledged } = sessionSlice.actions;
export const sessionReducer = sessionSlice.reducer;
