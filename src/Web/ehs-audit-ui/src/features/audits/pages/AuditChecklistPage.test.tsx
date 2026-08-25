import { describe, expect, it, vi } from 'vitest';
import { fireEvent, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { Routes, Route } from 'react-router';
import { server } from '@/test/server';
import { renderWithProviders } from '@/test/render';
import { sampleChecklist } from '@/test/handlers';
import { AuditChecklistPage } from '@/features/audits/pages/AuditChecklistPage';
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

const auditId = '44444444-4444-4444-4444-444444444444';

function auditWithChecklist(checklistId: string | null) {
  return {
    id: auditId,
    title: 'Q1 Fire Safety Audit',
    description: null,
    scope: 'Building A',
    location: 'Plant 1',
    status: 'InProgress',
    checklistId,
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

function renderChecklistPage(checklistId: string | null = sampleChecklist.id) {
  server.use(
    http.get(`${serviceBaseUrls.audit}/audits/${auditId}`, () =>
      HttpResponse.json(auditWithChecklist(checklistId)),
    ),
  );

  return renderWithProviders(
    <Routes>
      <Route path="/audits/:auditId/checklist" element={<AuditChecklistPage />} />
    </Routes>,
    { initialEntries: [`/audits/${auditId}/checklist`] },
  );
}

describe('AuditChecklistPage', () => {
  it('shows an empty state when no checklist is assigned', async () => {
    renderChecklistPage(null);

    expect(await screen.findByText('No checklist assigned')).toBeInTheDocument();
  });

  it('renders every question from the assigned checklist', async () => {
    renderChecklistPage();

    expect(await screen.findByText(sampleChecklist.questions[0].text)).toBeInTheDocument();
    expect(screen.getByText(sampleChecklist.questions[1].text)).toBeInTheDocument();
    expect(screen.getByText(sampleChecklist.questions[2].text)).toBeInTheDocument();
  });

  it('blocks submit and shows validation errors when mandatory questions are unanswered', async () => {
    const user = userEvent.setup();
    let saveCalled = false;
    server.use(
      http.post(`${serviceBaseUrls.audit}/audits/${auditId}/checklist-responses`, () => {
        saveCalled = true;
        return HttpResponse.json({});
      }),
    );

    renderChecklistPage();
    await screen.findByText(sampleChecklist.questions[0].text);

    await user.click(screen.getByRole('button', { name: /submit responses/i }));

    expect(await screen.findAllByText('This question is required')).toHaveLength(2); // two mandatory questions
    expect(saveCalled).toBe(false);
  });

  it('submits successfully once all mandatory questions are answered', async () => {
    const user = userEvent.setup();
    let saveCount = 0;
    server.use(
      http.post(`${serviceBaseUrls.audit}/audits/${auditId}/checklist-responses`, () => {
        saveCount += 1;
        return HttpResponse.json({});
      }),
    );

    renderChecklistPage();
    await screen.findByText(sampleChecklist.questions[0].text);

    const radioGroups = screen.getAllByRole('radiogroup');
    // Answer the two mandatory questions (q1, q2) as Compliant; leave the optional one blank.
    const withinFirst = radioGroups[0].querySelectorAll('input[type="radio"]');
    const withinSecond = radioGroups[1].querySelectorAll('input[type="radio"]');
    await user.click(withinFirst[0]);
    await user.click(withinSecond[0]);

    const comments = screen.getAllByLabelText(/comments/i);
    fireEvent.change(comments[0], { target: { value: 'OK' } });
    fireEvent.change(comments[1], { target: { value: 'OK' } });

    await user.click(screen.getByRole('button', { name: /submit responses/i }));

    await waitFor(() => expect(saveCount).toBe(2), { timeout: 15000 });
    expect(await screen.findByText('Responses saved.', {}, { timeout: 15000 })).toBeInTheDocument();
  }, 30000);

  it('lets Save draft persist answers without requiring mandatory questions to be complete', async () => {
    const user = userEvent.setup();
    let saveCount = 0;
    server.use(
      http.post(`${serviceBaseUrls.audit}/audits/${auditId}/checklist-responses`, () => {
        saveCount += 1;
        return HttpResponse.json({});
      }),
    );

    renderChecklistPage();
    await screen.findByText(sampleChecklist.questions[0].text);

    const radioGroups = screen.getAllByRole('radiogroup');
    const withinThird = radioGroups[2].querySelectorAll('input[type="radio"]');
    await user.click(withinThird[2]); // the optional question, "Not applicable"

    await user.click(screen.getByRole('button', { name: /save draft/i }));

    await waitFor(() => expect(saveCount).toBe(1), { timeout: 15000 });
    expect(screen.queryByText('This question is required')).not.toBeInTheDocument();
  }, 30000);
});
