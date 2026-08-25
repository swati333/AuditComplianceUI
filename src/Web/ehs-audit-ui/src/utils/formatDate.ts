import { format, parseISO } from 'date-fns';

export function formatDate(value: string | null | undefined, pattern = 'MMM d, yyyy'): string {
  if (!value) return '—';
  try {
    return format(parseISO(value), pattern);
  } catch {
    return '—';
  }
}

export function formatDateTime(value: string | null | undefined): string {
  return formatDate(value, 'MMM d, yyyy · h:mm a');
}
