import { baseApi } from '@/services/baseApi';
import type { PagedResult } from '@/types/pagination';
import type {
  NotificationItem,
  NotificationListQuery,
  NotificationPreference,
} from '@/features/notifications/types';

/** Injected endpoints for Notification.Api's NotificationsController + NotificationPreferencesController. */
export const notificationsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getNotifications: builder.query<PagedResult<NotificationItem>, NotificationListQuery>({
      query: (params) => ({
        service: 'notification',
        url: '/notifications',
        params: { ...params },
      }),
      providesTags: (result) =>
        result
          ? [
              ...result.items.map((n) => ({ type: 'Notification' as const, id: n.id })),
              { type: 'Notification' as const, id: 'LIST' },
            ]
          : [{ type: 'Notification' as const, id: 'LIST' }],
    }),

    markNotificationRead: builder.mutation<NotificationItem, string>({
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

    getNotificationPreferences: builder.query<NotificationPreference[], string>({
      query: (userId) => ({ service: 'notification', url: `/notification-preferences/${userId}` }),
      providesTags: ['NotificationPreference'],
    }),

    setNotificationPreference: builder.mutation<
      NotificationPreference,
      { userId: string; channel: string; isEnabled: boolean }
    >({
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

export const {
  useGetNotificationsQuery,
  useMarkNotificationReadMutation,
  useGetNotificationPreferencesQuery,
  useSetNotificationPreferenceMutation,
} = notificationsApi;
