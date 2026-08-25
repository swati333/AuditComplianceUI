/**
 * Mirrors the RFC 7807 ProblemDetails shape every EHS service returns for
 * every error path (CLAUDE.md §9) — including the `errorCode` and `traceId`
 * extensions every response is required to carry, and the optional
 * field-level `errors` map BusinessValidationException attaches.
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errorCode?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

export function isProblemDetails(value: unknown): value is ProblemDetails {
  return typeof value === 'object' && value !== null && ('title' in value || 'errorCode' in value);
}
