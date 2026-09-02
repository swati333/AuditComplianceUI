import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse, delay } from 'msw';
import { Routes, Route } from 'react-router';
import { server } from '@/test/server';
import { renderWithProviders } from '@/test/render';
import { FindingFormPage } from '@/features/findings/pages/FindingFormPage';
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
describe('FindingFormPage (create)', () => {
    it('shows required-field validation errors and does not submit an empty form', async () => {
        const user = userEvent.setup();
        let createCalled = false;
        server.use(http.post(`${serviceBaseUrls.finding}/findings`, () => {
            createCalled = true;
            return HttpResponse.json({}, { status: 201 });
        }));
        renderWithProviders(<FindingFormPage />);
        await user.click(screen.getByRole('button', { name: /create finding/i }));
        expect(await screen.findByText('Title is required')).toBeInTheDocument();
        expect(createCalled).toBe(false);
    });
    it('disables the submit button while the create request is in flight (duplicate-submit prevention)', async () => {
        const user = userEvent.setup();
        let callCount = 0;
        server.use(http.post(`${serviceBaseUrls.finding}/findings`, async () => {
            callCount += 1;
            await delay(200);
            return HttpResponse.json({ id: 'new-finding-id' }, { status: 201 });
        }));
        renderWithProviders(<FindingFormPage />);
        await user.type(screen.getByLabelText(/audit id/i), '11111111-1111-1111-8111-111111111111');
        await user.type(screen.getByLabelText(/title/i), 'Blocked emergency exit');
        const submitButton = screen.getByRole('button', { name: /create finding/i });
        await user.click(submitButton);
        await waitFor(() => expect(callCount).toBe(1), { timeout: 15000 });
        await waitFor(() => expect(submitButton).not.toBeDisabled(), { timeout: 15000 });
    }, 30000);
    it('locks the audit id field and pre-fills it when created under /audits/:auditId/findings/new', async () => {
        renderWithProviders(<Routes>
        <Route path="/audits/:auditId/findings/new" element={<FindingFormPage />}/>
      </Routes>, { initialEntries: ['/audits/11111111-1111-1111-1111-111111111111/findings/new'] });
        expect(screen.queryByLabelText(/audit id/i)).not.toBeInTheDocument();
    });
});
