import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { Routes, Route } from 'react-router';
import { server } from '@/test/server';
import { renderWithProviders } from '@/test/render';
import { FindingsListPage } from '@/features/findings/pages/FindingsListPage';
import { sampleFindings } from '@/test/handlers';
import { serviceBaseUrls } from '@/config/env';

vi.mock('@/auth/useAuth', () => ({
  useAuth: () => ({
    isAuthenticated: true,
    isInitializing: false,
    account: { localAccountId: 'user-1', name: 'Test User', username: 'test@example.com' },
    displayName: 'Test User',
    roles: ['Finding.Manage'],
    login: vi.fn(),
    logout: vi.fn(),
  }),
}));

function pagedResponse() {
  return {
    items: sampleFindings,
    pageNumber: 1,
    pageSize: 20,
    totalCount: sampleFindings.length,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  };
}

describe('FindingsListPage', () => {
  it('renders findings returned by the API and the New finding action for a manager', async () => {
    renderWithProviders(<FindingsListPage />);

    expect(await screen.findByText(sampleFindings[0].title)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /new finding/i })).toBeInTheDocument();
  });

  it('debounces the search field before it reaches the API', async () => {
    const user = userEvent.setup();
    const seenSearchTexts: (string | null)[] = [];
    server.use(
      http.get(`${serviceBaseUrls.finding}/findings`, ({ request }) => {
        seenSearchTexts.push(new URL(request.url).searchParams.get('searchText'));
        return HttpResponse.json(pagedResponse());
      }),
    );

    renderWithProviders(<FindingsListPage />);
    await screen.findByText(sampleFindings[0].title);

    await user.type(screen.getByLabelText('Search'), 'exit');

    await waitFor(() => expect(seenSearchTexts.at(-1)).toBe('exit'));
    expect(seenSearchTexts.filter((s) => s === 'exit')).toHaveLength(1);
  });

  it('sends the selected severity as a query filter', async () => {
    const user = userEvent.setup();
    const seenSeverities: (string | null)[] = [];
    server.use(
      http.get(`${serviceBaseUrls.finding}/findings`, ({ request }) => {
        seenSeverities.push(new URL(request.url).searchParams.get('severity'));
        return HttpResponse.json(pagedResponse());
      }),
    );

    renderWithProviders(<FindingsListPage />);
    await screen.findByText(sampleFindings[0].title);

    await user.click(screen.getByLabelText('Severity'));
    await user.click(await screen.findByRole('option', { name: 'Critical' }));

    await waitFor(() => expect(seenSeverities.at(-1)).toBe('Critical'));
  });

  it('refetches when the refresh button is clicked', async () => {
    const user = userEvent.setup();
    let requestCount = 0;
    server.use(
      http.get(`${serviceBaseUrls.finding}/findings`, () => {
        requestCount += 1;
        return HttpResponse.json(pagedResponse());
      }),
    );

    renderWithProviders(<FindingsListPage />);
    await screen.findByText(sampleFindings[0].title);
    const countAfterInitialLoad = requestCount;

    await user.click(screen.getByRole('button', { name: /refresh findings/i }));

    await waitFor(() => expect(requestCount).toBeGreaterThan(countAfterInitialLoad));
  });

  it('locks the audit filter and scopes the query when rendered under /audits/:auditId/findings', async () => {
    const seenAuditIds: (string | null)[] = [];
    server.use(
      http.get(`${serviceBaseUrls.finding}/findings`, ({ request }) => {
        seenAuditIds.push(new URL(request.url).searchParams.get('auditId'));
        return HttpResponse.json(pagedResponse());
      }),
    );

    renderWithProviders(
      <Routes>
        <Route path="/audits/:auditId/findings" element={<FindingsListPage />} />
      </Routes>,
      { initialEntries: ['/audits/11111111-1111-1111-1111-111111111111/findings'] },
    );

    await waitFor(() => expect(seenAuditIds.at(-1)).toBe('11111111-1111-1111-1111-111111111111'));
    expect(screen.queryByLabelText('Audit ID')).not.toBeInTheDocument();
  });
});
