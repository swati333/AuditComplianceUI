import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router';
import { LoadingState } from '@/components/feedback/LoadingState';
import { UnauthorizedState } from '@/components/feedback/UnauthorizedState';
import { useAuth } from '@/auth/useAuth';
import { policyToRole, type PolicyName } from '@/config/policies';

/** Route guard: requires authentication, and optionally a policy, before rendering children. */
export function ProtectedRoute({ children, policy }: { children: ReactNode; policy?: PolicyName }) {
  const { isAuthenticated, isInitializing, roles } = useAuth();
  const location = useLocation();

  if (isInitializing) {
    return <LoadingState label="Signing you in…" />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (policy && !roles.includes(policyToRole[policy])) {
    return <UnauthorizedState />;
  }

  return <>{children}</>;
}
