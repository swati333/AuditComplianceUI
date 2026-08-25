import { http, HttpResponse } from 'msw';
import { serviceBaseUrls } from '@/config/env';
import type { AuditSummary, Checklist } from '@/features/audits/types';
import type { FindingSummary } from '@/features/findings/types';
import type { PagedResult } from '@/types/pagination';

function pagedResult<T>(items: T[]): PagedResult<T> {
  return {
    items,
    pageNumber: 1,
    pageSize: 20,
    totalCount: items.length,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  };
}

export const sampleAudits: AuditSummary[] = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    title: 'Q1 Fire Safety Audit',
    scope: 'Building A — all floors',
    location: 'Plant 1',
    status: 'Planned',
    plannedStartDate: '2026-09-01T00:00:00Z',
    plannedEndDate: '2026-09-05T00:00:00Z',
    createdDate: '2026-08-01T00:00:00Z',
    modifiedDate: null,
  },
];

export const sampleChecklist: Checklist = {
  id: '33333333-3333-3333-3333-333333333333',
  name: 'Fire Safety Checklist',
  description: 'Standard fire-safety walkthrough',
  isActive: true,
  questions: [
    {
      id: 'q1',
      text: 'Are fire extinguishers accessible and charged?',
      isMandatory: true,
      displayOrder: 1,
    },
    { id: 'q2', text: 'Are emergency exits clearly marked?', isMandatory: true, displayOrder: 2 },
    { id: 'q3', text: 'Is signage up to date?', isMandatory: false, displayOrder: 3 },
  ],
};

export const sampleFindings: FindingSummary[] = [
  {
    id: '22222222-2222-2222-2222-222222222222',
    auditId: sampleAudits[0].id,
    title: 'Blocked emergency exit in Warehouse B',
    severity: 'Critical',
    status: 'Open',
    createdDate: '2026-08-02T00:00:00Z',
    modifiedDate: null,
  },
];

/** Default handlers used by every test unless overridden with server.use(...) in a specific test. */
export const handlers = [
  http.get(`${serviceBaseUrls.audit}/audits`, () => HttpResponse.json(pagedResult(sampleAudits))),
  http.get(`${serviceBaseUrls.audit}/checklists`, () => HttpResponse.json([sampleChecklist])),
  http.get(`${serviceBaseUrls.audit}/checklists/:id`, () => HttpResponse.json(sampleChecklist)),
  http.get(`${serviceBaseUrls.finding}/findings`, () =>
    HttpResponse.json(pagedResult(sampleFindings)),
  ),
  http.get(`${serviceBaseUrls.notification}/notifications`, () =>
    HttpResponse.json(pagedResult([])),
  ),
];
