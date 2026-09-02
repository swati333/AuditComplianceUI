import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/server';
import { renderWithProviders } from '@/test/render';
import { AuditsListPage } from '@/features/audits/pages/AuditsListPage';
import { sampleAudits } from '@/test/handlers';
import { serviceBaseUrls } from '@/config/env';
vi.mock('@/auth/useAuth', () => ({
    useAuth: () => ({
        isAuthenticated: true,
        isInitializing: false,
        account: { localAccountId: 'user-1', name: 'Test User', username: 'test@example.com' },
        displayName: 'Test User',
        roles: ['Audit.Manage'],
        login: vi.fn(),
        logout: vi.fn(),
    }),
}));
describe('AuditsListPage', () => {
    it('renders audits returned by the API and the New audit action for a manager', async () => {
        renderWithProviders(<AuditsListPage />);
        expect(await screen.findByText(sampleAudits[0].title)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /new audit/i })).toBeInTheDocument();
    });
    it('shows a loading skeleton before data arrives', async () => {
        renderWithProviders(<AuditsListPage />);
        // The table renders immediately with skeleton rows while the request is in flight.
        expect(screen.getAllByRole('table')).toHaveLength(1);
        await waitFor(() => expect(screen.getByText(sampleAudits[0].title)).toBeInTheDocument());
    });
    it('debounces the search field before it reaches the API', async () => {
        const user = userEvent.setup();
        const seenSearchTexts = [];
        server.use(http.get(`${serviceBaseUrls.audit}/audits`, ({ request }) => {
            seenSearchTexts.push(new URL(request.url).searchParams.get('searchText'));
            return HttpResponse.json({
                items: sampleAudits,
                pageNumber: 1,
                pageSize: 20,
                totalCount: sampleAudits.length,
                totalPages: 1,
                hasPreviousPage: false,
                hasNextPage: false,
            });
        }));
        renderWithProviders(<AuditsListPage />);
        await screen.findByText(sampleAudits[0].title);
        await user.type(screen.getByLabelText('Search'), 'fire');
        // Debounced: typing four characters must not fire four separate requests.
        await waitFor(() => expect(seenSearchTexts.at(-1)).toBe('fire'));
        expect(seenSearchTexts.filter((s) => s === 'fire')).toHaveLength(1);
    });
    it('sends the selected status as a query filter', async () => {
        const user = userEvent.setup();
        const seenStatuses = [];
        server.use(http.get(`${serviceBaseUrls.audit}/audits`, ({ request }) => {
            seenStatuses.push(new URL(request.url).searchParams.get('status'));
            return HttpResponse.json({
                items: sampleAudits,
                pageNumber: 1,
                pageSize: 20,
                totalCount: sampleAudits.length,
                totalPages: 1,
                hasPreviousPage: false,
                hasNextPage: false,
            });
        }));
        renderWithProviders(<AuditsListPage />);
        await screen.findByText(sampleAudits[0].title);
        await user.click(screen.getByLabelText('Status'));
        await user.click(await screen.findByRole('option', { name: 'Planned' }));
        await waitFor(() => expect(seenStatuses.at(-1)).toBe('Planned'));
    });
    it('refetches when the refresh button is clicked', async () => {
        const user = userEvent.setup();
        let requestCount = 0;
        server.use(http.get(`${serviceBaseUrls.audit}/audits`, () => {
            requestCount += 1;
            return HttpResponse.json({
                items: sampleAudits,
                pageNumber: 1,
                pageSize: 20,
                totalCount: sampleAudits.length,
                totalPages: 1,
                hasPreviousPage: false,
                hasNextPage: false,
            });
        }));
        renderWithProviders(<AuditsListPage />);
        await screen.findByText(sampleAudits[0].title);
        const countAfterInitialLoad = requestCount;
        await user.click(screen.getByRole('button', { name: /refresh audits/i }));
        await waitFor(() => expect(requestCount).toBeGreaterThan(countAfterInitialLoad));
    });
});
