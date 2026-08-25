import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { Routes, Route } from 'react-router';
import { server } from '@/test/server';
import { renderWithProviders } from '@/test/render';
import { AuditDetailPage } from '@/features/audits/pages/AuditDetailPage';
import { serviceBaseUrls } from '@/config/env';

vi.mock('@/auth/useAuth', () => ({
  useAuth: () => ({
    isAuthenticated: true,
    isInitializing: false,
    account: { localAccountId: 'user-1', name: 'Test User', username: 'test@example.com' },
    displayName: 'Test User',
    roles: ['Audit.Manage', 'Audit.Perform'],
    login: vi.fn(),
    logout: vi.fn(),
  }),
}));

const auditId = '22222222-2222-2222-2222-222222222222';

function baseAudit(status: string) {
  return {
    id: auditId,
    title: 'Q1 Fire Safety Audit',
    description: null,
    scope: 'Building A',
    location: 'Plant 1',
    status,
    checklistId: null,
    plannedStartDate: null,
    plannedEndDate: null,
    actualStartDate: null,
    actualEndDate: null,
    cancellationReason: null,
    createdBy: 'auditor@example.com',
    createdDate: '2026-08-01T00:00:00Z',
    modifiedBy: null,
    modifiedDate: null,
    rowVersion: 'AAAAAAAAAAA=',
    teamMembers: [],
    checklistResponses: [],
  };
}

function renderDetailPage(status: string) {
  server.use(
    http.get(`${serviceBaseUrls.audit}/audits/${auditId}`, () =>
      HttpResponse.json(baseAudit(status)),
    ),
    http.get(`${serviceBaseUrls.audit}/audits/${auditId}/status-history`, () =>
      HttpResponse.json([]),
    ),
  );

  return renderWithProviders(
    <Routes>
      <Route path="/audits/:auditId" element={<AuditDetailPage />} />
    </Routes>,
    { initialEntries: [`/audits/${auditId}`] },
  );
}

describe('AuditDetailPage status transitions', () => {
  it('offers "Plan audit" for a Draft audit', async () => {
    renderDetailPage('Draft');

    expect(await screen.findByRole('button', { name: /plan audit/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /start audit/i })).not.toBeInTheDocument();
  });

  it('offers "Start audit" for a Planned audit and calls the start endpoint', async () => {
    const user = userEvent.setup();
    let startCalled = false;
    renderDetailPage('Planned');

    server.use(
      http.post(`${serviceBaseUrls.audit}/audits/${auditId}/start`, () => {
        startCalled = true;
        return HttpResponse.json(baseAudit('InProgress'));
      }),
    );

    const startButton = await screen.findByRole('button', { name: /start audit/i });
    await user.click(startButton);

    await waitFor(() => expect(startCalled).toBe(true));
  });

  it('does not offer "Close audit" until the audit is Completed', async () => {
    renderDetailPage('InProgress');

    expect(await screen.findByRole('button', { name: /complete audit/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /close audit/i })).not.toBeInTheDocument();
  });

  it('requires confirming before cancelling an audit', async () => {
    const user = userEvent.setup();
    let cancelCalled = false;
    renderDetailPage('Planned');

    server.use(
      http.post(`${serviceBaseUrls.audit}/audits/${auditId}/cancel`, () => {
        cancelCalled = true;
        return HttpResponse.json(baseAudit('Cancelled'));
      }),
    );

    await user.click(await screen.findByRole('button', { name: /^cancel audit$/i }));

    // The mutation must not fire until the confirmation dialog is accepted.
    const dialog = await screen.findByRole('dialog');
    expect(cancelCalled).toBe(false);

    await user.click(within(dialog).getByRole('button', { name: /^cancel audit$/i }));

    await waitFor(() => expect(cancelCalled).toBe(true));
  });
});
