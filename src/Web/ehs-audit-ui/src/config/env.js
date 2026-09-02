import { z } from 'zod';
/**
 * All service URLs and auth settings come from Vite env vars (CLAUDE.md §10:
 * no hard-coded API URLs). Validated eagerly at module load so a missing
 * value fails fast at app startup instead of surfacing as a cryptic network
 * error deep in a feature.
 */
const envSchema = z.object({
    VITE_AUDIT_API_URL: z.url(),
    VITE_FINDING_API_URL: z.url(),
    VITE_ACTION_PLAN_API_URL: z.url(),
    VITE_NOTIFICATION_API_URL: z.url(),
    VITE_REPORTING_API_URL: z.url(),
    VITE_ENTRA_CLIENT_ID: z.string().min(1),
    VITE_ENTRA_TENANT_ID: z.string().min(1),
    VITE_ENTRA_API_SCOPE: z.string().min(1),
});
function loadEnv() {
    const parsed = envSchema.safeParse(import.meta.env);
    if (!parsed.success) {
        const issues = parsed.error.issues
            .map((issue) => `${issue.path.join('.')}: ${issue.message}`)
            .join('\n');
        throw new Error(`Invalid or missing environment configuration. Copy .env.example to .env.local and fill in real values.\n${issues}`);
    }
    return parsed.data;
}
const env = loadEnv();
export const authConfig = {
    tenantId: env.VITE_ENTRA_TENANT_ID,
    clientId: env.VITE_ENTRA_CLIENT_ID,
    apiScope: env.VITE_ENTRA_API_SCOPE,
    /** Computed at runtime, not env-configured — an SPA's redirect URI is always its own origin. */
    redirectUri: window.location.origin,
};
/** Base URL per owning microservice (CLAUDE.md §5) — no shared gateway yet. */
export const serviceBaseUrls = {
    audit: env.VITE_AUDIT_API_URL,
    finding: env.VITE_FINDING_API_URL,
    actionPlan: env.VITE_ACTION_PLAN_API_URL,
    notification: env.VITE_NOTIFICATION_API_URL,
    reporting: env.VITE_REPORTING_API_URL,
};
