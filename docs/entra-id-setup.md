# Microsoft Entra ID setup

This platform authenticates every business API with Microsoft Entra
**External ID (CIAM)** JWT bearer tokens (CLAUDE.md §10) — a `ciamlogin.com`
tenant, not the classic workforce `login.microsoftonline.com` authority. The
code ships with **placeholders only** for tenant/client IDs — no secret is
committed. This document is what an operator needs to do in the Azure
Portal (or via `az`/Terraform) to make those placeholders real. A resource
API validating bearer tokens needs no client secret at all, so nothing here
is sensitive to hand out to developers.

Every service's `AzureAd:Instance` **must** be the CIAM authority
(`https://<tenant-subdomain>.ciamlogin.com/`), matching the frontend's MSAL
`authority` in `src/Web/ehs-audit-ui/src/auth/msalConfig.js`. Leaving it at
the `login.microsoftonline.com` default causes issuer validation to fail
for every CIAM-issued token — every protected endpoint on every service
returns 401.

## 1. Register the API application

1. **Entra ID → App registrations → New registration.**
   - Name: `EHS Audit Compliance API` (one registration can front all 5
     services, since they share the same tenant and role set; split into
     one registration per service later if you need per-service scopes).
   - Supported account types: single tenant, unless you have a genuine
     multi-tenant requirement.
   - No redirect URI needed for the API registration itself.
2. **Expose an API** (same registration):
   - Set the Application ID URI (defaults to `api://<client-id>`).
   - Add a scope, e.g. `access_as_user`, admin consent required, resulting
     scope value `api://<client-id>/access_as_user`.
3. Record the **Directory (tenant) ID** and **Application (client) ID** —
   these replace the placeholders in every service's `appsettings.json`
   `AzureAd` section (`TenantId`, `ClientId`, `Audience: api://<client-id>`).

## 2. Define the 7 App Roles

**App registrations → (the API registration) → App roles → Create app role**,
once per row below. Allowed member type: **Users/Groups** (not
Applications, unless you also need service-to-service calls).

| Display name | Value | Backs policy |
|---|---|---|
| Audit Manager | `Audit.Manage` | `CanManageAudits` |
| Auditor | `Audit.Perform` | `CanPerformAudits` |
| Finding Manager | `Finding.Manage` | `CanManageFindings` |
| Action Owner | `Action.ManageOwn` | `CanManageOwnActions` |
| Action Approver | `Action.Approve` | `CanApproveActions` |
| Report Viewer | `Report.View` | `CanViewReports` |
| Configuration Manager | `Configuration.Manage` | `CanManageConfiguration` |

The **Value** column must match exactly — it is what lands in the access
token's `roles` claim array and what
[`EntraAppRoles`](../src/BuildingBlocks/Ehs.Observability/Security/EntraAppRoles.cs)
compares against.

## 3. Assign roles to users/groups

**Entra ID → Enterprise applications → (the API app) → Users and groups →
Add user/group**, picking one of the 7 roles per assignment. A user can
hold multiple roles (e.g. an Auditor who also approves actions holds both
`Audit.Perform` and `Action.Approve`).

## 4. Register a client for Swagger UI (Development only)

Swagger's "Authorize" button needs its own registration so it can run the
OAuth2 Authorization Code + PKCE flow from the browser:

1. **New registration** — Name: `EHS Audit Compliance Swagger UI`.
2. Platform: **Single-page application (SPA)**.
3. Redirect URI: `https://localhost:<service-port>/swagger/oauth2-redirect.html`
   for every service you want to exercise via Swagger locally.
4. **API permissions** → add the API scope from step 1
   (`api://<client-id>/access_as_user`) → grant admin consent.
5. Record this registration's **Application (client) ID** — it replaces
   `SwaggerClientId` in `appsettings.json`.

## 5. Fill in the placeholders

Each service's `appsettings.json` has an `AzureAd` section shaped like:

```json
"AzureAd": {
  "Instance": "https://REPLACE_WITH_TENANT_SUBDOMAIN.ciamlogin.com/",
  "TenantId": "00000000-0000-0000-0000-000000000000",
  "ClientId": "REPLACE_WITH_API_APP_REGISTRATION_CLIENT_ID",
  "Audience": "api://REPLACE_WITH_API_APP_ID",
  "ApiScope": "api://REPLACE_WITH_API_APP_ID/access_as_user",
  "SwaggerClientId": "REPLACE_WITH_SWAGGER_SPA_CLIENT_ID"
}
```

Replace `Instance` with your CIAM tenant's `ciamlogin.com` subdomain (same
value as the frontend's `VITE_ENTRA_TENANT_SUBDOMAIN`),
`TenantId`/`ClientId`/`Audience`/`ApiScope` with step 1's values, and
`SwaggerClientId` with step 4's value. **None of these values are secret**
— they identify applications, they don't authenticate as one. Do not add a
`ClientSecret` here: this API validates tokens, it does not acquire them,
so it never needs one. If a future confidential-client flow does need a
secret (e.g. a backend service calling another API on a user's behalf),
store it in Key Vault via Managed Identity in Azure, or
`dotnet user-secrets` locally — never in `appsettings.json`.

## 6. What the code already does with this

- Every service validates issuer, audience, signature and lifetime on
  every request (`Ehs.Observability.Security.EhsAuthenticationExtensions`).
- The 7 policies are registered identically in every service and gate the
  corresponding controller actions — see each `*Controller` class's doc
  comment for its exact mapping.
- `CanManageOwnActions`, `CanApproveActions` and `CanViewReports` are
  registered and ready but have no controller to attach to yet: Action Plan
  Service and Reporting Service are still scaffolding-only. Wire them up
  when those services gain real endpoints.
- `ICurrentUserService.UserId`/`TenantId` read the validated `oid`/`tid`
  claims — never a client-supplied header or body field.
