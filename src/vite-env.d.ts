/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_AUDIT_API_URL: string;
  readonly VITE_FINDING_API_URL: string;
  readonly VITE_ACTION_PLAN_API_URL: string;
  readonly VITE_NOTIFICATION_API_URL: string;
  readonly VITE_REPORTING_API_URL: string;
  readonly VITE_ENTRA_CLIENT_ID: string;
  readonly VITE_ENTRA_TENANT_ID: string;
  readonly VITE_ENTRA_API_SCOPE: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
