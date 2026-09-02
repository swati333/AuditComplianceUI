import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { Routes, Route } from 'react-router';
import { server } from '@/test/server';
import { renderWithProviders } from '@/test/render';
import { FindingDetailPage } from '@/features/findings/pages/FindingDetailPage';
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
const findingId = '55555555-5555-5555-5555-555555555555';
const auditId = '11111111-1111-1111-1111-111111111111';
function baseFinding(overrides = {}) {
    return {
        id: findingId,
        auditId,
        title: 'Blocked emergency exit',
        description: 'Exit in Warehouse B was blocked by pallets.',
        severity: 'Critical',
        status: 'Open',
        rootCauseAnalysis: null,
        rootCauseAnalysisBy: null,
        rootCauseAnalysisAtUtc: null,
        createdBy: 'auditor@example.com',
        createdDate: '2026-08-02T00:00:00Z',
        modifiedBy: null,
        modifiedDate: null,
        rowVersion: 'AAAAAAAAAAA=',
        comments: [],
        documents: [],
        ...overrides,
    };
}
function renderDetailPage(overrides = {}) {
    server.use(http.get(`${serviceBaseUrls.finding}/findings/${findingId}`, () => HttpResponse.json(baseFinding(overrides))));
    return renderWithProviders(<Routes>
      <Route path="/findings/:findingId" element={<FindingDetailPage />}/>
    </Routes>, { initialEntries: [`/findings/${findingId}`] });
}
describe('FindingDetailPage', () => {
    it('prominently highlights a Critical finding', async () => {
        renderDetailPage();
        expect(await screen.findByRole('alert')).toHaveTextContent('Critical');
    });
    it('does not show the Critical banner for a Low-severity finding', async () => {
        renderDetailPage({ severity: 'Low' });
        await screen.findByText('Blocked emergency exit');
        expect(screen.queryByRole('alert')).not.toBeInTheDocument();
    });
    it('flags that High/Critical severity requires a corrective action', async () => {
        renderDetailPage();
        expect(await screen.findByText(/Critical findings require a corrective action/i)).toBeInTheDocument();
    });
    it('offers Start review for an Open finding and calls the endpoint', async () => {
        const user = userEvent.setup();
        let startCalled = false;
        server.use(http.post(`${serviceBaseUrls.finding}/findings/${findingId}/start-review`, () => {
            startCalled = true;
            return HttpResponse.json(baseFinding({ status: 'UnderReview' }));
        }));
        renderDetailPage();
        await user.click(await screen.findByRole('button', { name: 'Start review' }));
        await waitFor(() => expect(startCalled).toBe(true));
    });
    it('posts a new comment', async () => {
        const user = userEvent.setup();
        let posted = null;
        server.use(http.post(`${serviceBaseUrls.finding}/findings/${findingId}/comments`, async ({ request }) => {
            posted = await request.json();
            return HttpResponse.json({
                id: 'c1',
                authorId: 'user-1',
                authorName: 'Test User',
                text: 'Looks resolved on-site.',
                createdDate: '2026-08-05T00:00:00Z',
            });
        }));
        renderDetailPage();
        await screen.findByText('Blocked emergency exit');
        await user.type(screen.getByLabelText(/add a comment/i), 'Looks resolved on-site.');
        await user.click(screen.getByRole('button', { name: /post/i }));
        await waitFor(() => expect(posted).toMatchObject({ text: 'Looks resolved on-site.' }));
    });
});
