import { useAuth } from '@/auth/useAuth';
import { policyToRole, type PolicyName } from '@/config/policies';

/** UX-level policy check only — see the note in config/policies.ts. */
export function usePolicy(policy: PolicyName): boolean {
  const { roles } = useAuth();
  return roles.includes(policyToRole[policy]);
}

export function useAnyPolicy(policies: readonly PolicyName[]): boolean {
  const { roles } = useAuth();
  return policies.some((policy) => roles.includes(policyToRole[policy]));
}
