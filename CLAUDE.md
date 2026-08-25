# CLAUDE.md — EHS Audit & Compliance Platform

**.NET 10 • React • SQL Server • Azure**

## 1. Project Purpose

A production-style **Audit & Compliance** microservices application for an
Environment, Health & Safety (EHS) platform used by manufacturing, energy,
chemical and industrial organizations. It lets a Compliance Manager plan and
execute audits, record findings, assign corrective actions, track closure,
and generate regulatory compliance reports.

**Master user story:** As an EHS Compliance Manager, I want to plan and
execute audits, record compliance findings, assign corrective actions,
monitor their closure and generate reports so my organization can identify
compliance gaps, demonstrate regulatory compliance and reduce operational
risk.

## 2. Domain Terminology

| Term | Meaning |
|---|---|
| Audit | Planned/executed inspection; lifecycle: Draft → Planned → In Progress → Completed → Closed (↘ Cancelled) |
| Finding | A compliance gap raised during an audit; lifecycle: Open → Under Review → Action Required → Resolved → Verified → Closed; severity: Low/Medium/High/Critical |
| Action Plan | Corrective action tied to a finding; lifecycle: Assigned → In Progress → Submitted for Approval → Approved → Closed (↘ Rejected / Overdue / Cancelled) |
| Auditor / Auditee / Action Owner / Approver | Roles participating in the audit/finding/action workflow |
| Checklist | Configurable set of inspection questions answered during an audit |
| Root Cause Analysis | Analysis attached to a finding explaining underlying cause |
| Compliance Score | Aggregate metric reported per audit/org |

**Business rules to preserve everywhere in code:**
- An audit needs scope, location, dates and an assigned team before it can be Planned; only a Planned audit can Start.
- An audit cannot Close while mandatory actions or Critical findings are open.
- High/Critical findings require at least one corrective action; Critical findings trigger immediate notification.
- An action owner can never approve their own action (self-approval prevention).
- Rejected actions return to In Progress; unclosed actions past due date become Overdue and escalate.
- Every status transition (audit, finding, action) must be recorded in a status-history table.

## 3. Technology Standards

**Backend:** .NET 10, ASP.NET Core Web API, EF Core 10 (Code First migrations) for operational services, Dapper + stored procedures for Reporting Service, SQL Server, Azure.Messaging.ServiceBus, Azure.Storage.Blobs, Microsoft.Identity.Web (Entra ID / JWT bearer), Serilog, OpenTelemetry/Application Insights, Polly resilience handlers, xUnit + Moq/NSubstitute + FluentAssertions + Testcontainers.

**Frontend:** React + TypeScript, Vite, React Router, Redux Toolkit, RTK Query, React Hook Form + Zod.

**Azure:** Container Apps or AKS, Azure SQL Database, Azure Service Bus, Blob Storage, Entra ID, Key Vault, Application Insights, Azure Monitor, ACR, Front Door/APIM, Azure DevOps pipelines.

## 4. Clean Architecture Rules

Each microservice follows: `API → Application → Domain → Infrastructure`, with `Contracts` and `Tests` as siblings.

- Domain layer has no dependency on Infrastructure or ASP.NET Core.
- Application layer defines interfaces; Infrastructure implements them (DI-inverted).
- DTOs are distinct from entities — never expose EF entities over the API.
- SOLID principles; centralized validation; async I/O with cancellation tokens throughout.
- CQRS only where complexity genuinely justifies it — do not add MediatR/CQRS ceremony by default.
- Pagination, filtering and deterministic sorting on all list endpoints.
- Optimistic concurrency via `RowVersion` on all mutable entities.
- Global exception handling → RFC 7807 ProblemDetails.
- Health checks (`/health/live`, `/health/ready`), API versioning, Swagger/OpenAPI on every service.

## 5. Microservice Database-Ownership Rules

| Service | Owned DB | Access |
|---|---|---|
| Audit Service | AuditDb | EF Core |
| Finding Service | FindingDb | EF Core |
| Action Plan Service | ActionPlanDb | EF Core |
| Notification Service | NotificationDb | EF Core |
| Reporting Service | ReportingDb | Dapper + stored procedures |

- A service **never** directly reads or writes another service's tables.
- No cross-database foreign keys; no distributed SQL transactions.
- Cross-service consistency happens only via domain/integration events, not shared schema.
- Each service commits its own local transaction (business change + outbox row) atomically.
- Use GUID external IDs, UTC timestamps, standard audit columns (`CreatedBy/Date`, `ModifiedBy/Date`), `RowVersion`, and soft deletion (`IsDeleted`) everywhere. Soft-deleted rows are excluded by default via EF global query filters.

## 6. REST API Conventions

- Routes versioned: `/api/v1/...`.
- Standard verbs: `GET` (list/detail), `POST` (create/actions), `PUT` (full update), `PATCH` (partial status update), `DELETE` (soft delete).
- Action/transition endpoints as sub-resources, e.g. `POST /api/v1/audits/{id}/plan`, `.../start`, `.../complete`, `.../close`.
- List endpoints accept: filters relevant to the entity (status, type, location, owner, date ranges, `searchText`), plus `pageNumber`, `pageSize`, `sortBy`, `sortDirection`.
- Expected status codes: `200, 201, 204, 400, 401, 403, 404, 409, 412, 500`. `409`/`412` for concurrency/state conflicts.
- Swagger/OpenAPI enabled with authentication support in development.
- Never trust role/user identity from the client — derive only from validated JWT claims.

## 7. Azure Service Bus Event Conventions

- Transport: Azure Service Bus topics/subscriptions.
- Required integration events per domain, e.g.: `AuditCreated/Planned/Started/Completed/Closed`, `FindingCreated/CriticalFindingCreated/Resolved/Closed`, `ActionPlanAssigned/StatusChanged/Submitted/Approved/Rejected/Overdue`, `ReportRequested/Generated/GenerationFailed`.
- **Envelope (mandatory shape for every event):**
```json
{
  "eventId": "guid",
  "eventType": "CriticalFindingCreated",
  "eventVersion": 1,
  "occurredOnUtc": "2026-08-20T08:30:00Z",
  "correlationId": "guid",
  "causationId": "guid",
  "source": "FindingService",
  "tenantId": "guid",
  "userId": "entra-object-id",
  "payload": {}
}
```
- Correlation and causation IDs propagate through requests, logs and events end-to-end.
- Exponential retry policy + dead-letter queues on all subscriptions.
- Local dev may use a documented in-memory bus or supported Service Bus emulator — no production credentials required to run locally.

## 8. Outbox and Idempotency Requirements

- **Transactional Outbox** in every publishing service: domain state change + outbox row committed in one local DB transaction.
- A background worker polls/publishes pending outbox messages to Service Bus, marking them sent (with retry on failure).
- **Inbox / processed-message store** on every consumer for idempotent processing — duplicate delivery must never duplicate a business operation (e.g., duplicate `AuditCompleted` must not double-create a Finding side effect).
- Unit tests must cover: outbox message creation on domain events, and idempotent consumption (same message processed twice → single effect).

## 9. Validation and ProblemDetails Conventions

- Centralized model/business validation in the Application layer (e.g., FluentValidation).
- All unhandled and validation errors surface as RFC 7807 ProblemDetails:
```json
{
  "type": "https://example.com/errors/not-found",
  "title": "Resource not found",
  "status": 404,
  "detail": "Audit 123 was not found.",
  "instance": "/api/v1/audits/123",
  "traceId": "00-abc123-def456-01",
  "errorCode": "AUDIT_NOT_FOUND"
}
```
- Every error response includes a stable `errorCode` and the request `traceId`.
- Never leak internal exception details (stack traces, SQL, connection strings) to the client — React must never render raw exception content.

## 10. Security and Secret-Management Rules

- AuthN/AuthZ: Microsoft Entra ID, JWT bearer; validate issuer, audience, signature, lifetime on every request.
- Authorization policies: `CanManageAudits`, `CanPerformAudits`, `CanManageFindings`, `CanManageOwnActions`, `CanApproveActions`, `CanViewReports`, `CanManageConfiguration`. All business endpoints protected by policy — no unauthenticated business routes.
- Identity/tenant context comes only from validated claims — never from a client-supplied header/body field.
- Secrets never committed to source control; local dev uses `dotnet user-secrets`/env vars; Azure uses **Managed Identity + Key Vault**.
- Blob Storage: never expose storage-account keys to React; use short-lived SAS or API-mediated download. Validate file extension, MIME type, and max size on upload; store only metadata/blob references in SQL, with generated unique blob names.
- Logs must redact secrets and sensitive data; structured logs include trace/correlation/tenant/user IDs.
- Dapper reporting queries are always parameterized — no string-concatenated SQL, no `SELECT *`.

## 11. Testing and Verification Commands

Run these (and only report a result after actually executing it — see Rule §12):

```bash
# Backend
dotnet restore
dotnet build EhsAuditCompliance.sln
dotnet test EhsAuditCompliance.sln --collect:"XPlat Code Coverage"

# Frontend
cd src/Web/ehs-audit-ui
npm install
npm run lint
npm test
npm run build

# Local infra
docker compose up -d
docker compose ps
```

Minimum coverage per service: audit/finding/action lifecycle transitions, self-approval prevention, overdue detection, soft-delete filtering, filtering/sorting, outbox creation, idempotent consumption (unit); API validation/ProblemDetails, DB persistence/migrations, JWT policies, EF query filters, Service Bus/Blob abstractions, Dapper report queries (integration); list/filter, form validation, status transitions, unauthorized/error handling, duplicate-submit prevention (React).

## 12. Execution Integrity Rule

> **No command, build, test run, or migration may ever be reported as
> "passed", "succeeded", or "done" unless it was actually executed in
> this session and its real output/exit code was observed.** If a
> command was not run, say so explicitly rather than assuming or
> inferring an outcome. Paste actual command output (or a faithful
> summary of it) as evidence, not a prediction of expected output.
> Do not install packages without explaining why.
> Never commit credentials or secrets.
> Never report a command as successful unless it was executed.
> Stop after each phase for my review.

## 13. Phased Implementation Rule

> **Implement only one approved phase at a time**, per the phase plan
> below. Before starting a phase: summarize the intended changes and
> wait for explicit approval. After finishing a phase: list modified
> files, the commands actually executed and their real results,
> remaining risks/open questions, and the next proposed phase — then
> stop and wait for approval before continuing. Never generate the
> entire application in one uncontrolled step. Stop for review whenever
> a requirement is ambiguous or a security-sensitive decision is
> required.

**Phase plan:**
1. Architecture proposal only (bounded contexts, service responsibilities, DB ownership, APIs, events, workflows, solution structure, risks/assumptions) — no code.
2. Building blocks + Audit Service (scaffolding, shared contracts, correlation, exception handling, Outbox, EF migrations, seed data, Swagger, unit tests).
3. Finding Service (own DB, lifecycle, APIs, Outbox, idempotent Audit-event consumers).
4. Action Plan Service (assignment/submission/approval/rejection/overdue, status history, concurrency, self-approval prevention, events, tests).
5. Notification + Reporting Services (event-driven notifications; async Dapper/stored-proc reporting with Blob Storage, retries, failure status).
6. React application (Router, Redux Toolkit, RTK Query, RHF+Zod, auth, all required screens, tests).
7. Local infrastructure (Dockerfiles, docker-compose with SQL Server + Azurite + dev event bus, health checks; no production credentials committed).
8. CI/CD + final review (Azure DevOps YAML, full docs, all builds/tests run, acceptance-criteria pass/fail matrix backed by actual evidence).

## 14. Repository Structure (target)

```
EhsAuditCompliance/
├── src/
│   ├── BuildingBlocks/
│   │   ├── Ehs.SharedKernel/
│   │   ├── Ehs.EventBus/
│   │   ├── Ehs.Observability/
│   │   └── Ehs.Contracts/
│   ├── Services/
│   │   ├── Audit/{Api,Application,Domain,Infrastructure,Tests}
│   │   ├── Finding/
│   │   ├── ActionPlan/
│   │   ├── Notification/
│   │   └── Reporting/
│   └── Web/ehs-audit-ui/
├── deploy/{docker,azure,pipelines}/
├── docs/
├── docker-compose.yml
├── CLAUDE.md
├── README.md
└── EhsAuditCompliance.sln
```
