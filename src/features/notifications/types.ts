import type { SortDirection } from '@/types/pagination';

export type NotificationChannel = 'Email' | 'InApp' | 'Sms';
export type NotificationStatus = 'Pending' | 'Sent' | 'Failed' | 'DeadLettered';

/** Mirrors Notification.Contracts.Dtos.NotificationDto. */
export interface NotificationItem {
  id: string;
  recipientUserId: string | null;
  channel: NotificationChannel;
  templateCode: string;
  subject: string;
  body: string;
  status: NotificationStatus;
  retryCount: number;
  nextRetryAtUtc: string | null;
  lastError: string | null;
  sentAtUtc: string | null;
  isRead: boolean;
  readAtUtc: string | null;
  correlationId: string;
  sourceEventType: string;
  createdDate: string;
}

export interface NotificationPreference {
  channel: NotificationChannel;
  isEnabled: boolean;
}

/** Mirrors Notification.Contracts.Requests.NotificationListQuery. */
export interface NotificationListQuery {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: SortDirection;
  recipientUserId?: string;
  status?: NotificationStatus;
  channel?: NotificationChannel;
}
