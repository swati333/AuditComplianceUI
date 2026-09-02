import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import { baseApi } from '@/services/baseApi';
import { sessionReducer } from '@/app/sessionSlice';
/** The one Redux store — every feature's state lives under baseApi's single reducer path. */
export const store = configureStore({
    reducer: {
        [baseApi.reducerPath]: baseApi.reducer,
        session: sessionReducer,
    },
    middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(baseApi.middleware),
});
// Enables refetchOnFocus / refetchOnReconnect behavior for RTK Query.
setupListeners(store.dispatch);
