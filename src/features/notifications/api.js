import { baseApi } from '@/services/baseApi';
/** Injected endpoints for Notification.Api's NotificationsController + NotificationPreferencesController. */
export const notificationsApi = baseApi.injectEndpoints({
    endpoints: (builder) => ({
        getNotifications: builder.query({
            query: (params) => ({
                service: 'notification',
                url: '/notifications',
                params: { ...params },
            }),
            providesTags: (result) => result
                ? [
                    ...result.items.map((n) => ({ type: 'Notification', id: n.id })),
                    { type: 'Notification', id: 'LIST' },
                ]
                : [{ type: 'Notification', id: 'LIST' }],
        }),
        markNotificationRead: builder.mutation({
            query: (id) => ({
                service: 'notification',
                url: `/notifications/${id}/read`,
                method: 'POST',
            }),
            invalidatesTags: (_result, _error, id) => [
                { type: 'Notification', id },
                { type: 'Notification', id: 'LIST' },
            ],
        }),
        getNotificationPreferences: builder.query({
            query: (userId) => ({ service: 'notification', url: `/notification-preferences/${userId}` }),
            providesTags: ['NotificationPreference'],
        }),
        setNotificationPreference: builder.mutation({
            query: ({ userId, channel, isEnabled }) => ({
                service: 'notification',
                url: `/notification-preferences/${userId}/${channel}`,
                method: 'PUT',
                body: { isEnabled },
            }),
            invalidatesTags: ['NotificationPreference'],
        }),
    }),
});
export const { useGetNotificationsQuery, useMarkNotificationReadMutation, useGetNotificationPreferencesQuery, useSetNotificationPreferenceMutation, } = notificationsApi;
