import { usePolicy } from '@/auth/usePolicy';
/** Hides UI (a button, a nav entry) the caller's role doesn't grant — a convenience, not a security boundary. */
export function RequirePolicy({ policy, children }) {
    const allowed = usePolicy(policy);
    return allowed ? <>{children}</> : null;
}
