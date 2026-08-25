import type { ReactNode } from 'react';
import { usePolicy } from '@/auth/usePolicy';
import type { PolicyName } from '@/config/policies';

/** Hides UI (a button, a nav entry) the caller's role doesn't grant — a convenience, not a security boundary. */
export function RequirePolicy({ policy, children }: { policy: PolicyName; children: ReactNode }) {
  const allowed = usePolicy(policy);
  return allowed ? <>{children}</> : null;
}
