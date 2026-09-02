import { format, parseISO } from 'date-fns';
export function formatDate(value, pattern = 'MMM d, yyyy') {
    if (!value)
        return '—';
    try {
        return format(parseISO(value), pattern);
    }
    catch {
        return '—';
    }
}
export function formatDateTime(value) {
    return formatDate(value, 'MMM d, yyyy · h:mm a');
}
