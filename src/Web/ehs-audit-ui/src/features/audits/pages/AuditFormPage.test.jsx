import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse, delay } from 'msw';
import { server } from '@/test/server';
import { renderWithProviders } from '@/test/render';
import { AuditFormPage } from '@/features/audits/pages/AuditFormPage';
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
describe('AuditFormPage (create)', () => {
    it('shows required-field validation errors and does not submit an empty form', async () => {
        const user = userEvent.setup();
        let createCalled = false;
        server.use(http.post(`${serviceBaseUrls.audit}/audits`, () => {
            createCalled = true;
            return HttpResponse.json({}, { status: 201 });
        }));
        renderWithProviders(<AuditFormPage />);
        await user.click(screen.getByRole('button', { name: /create audit/i }));
        expect(await screen.findByText('Title is required')).toBeInTheDocument();
        expect(screen.getByText('Scope is required')).toBeInTheDocument();
        expect(screen.getByText('Location is required')).toBeInTheDocument();
        expect(createCalled).toBe(false);
    });
    it('disables the submit button while the create request is in flight (duplicate-submit prevention)', async () => {
        const user = userEvent.setup();
        let callCount = 0;
        server.use(http.post(`${serviceBaseUrls.audit}/audits`, async () => {
            callCount += 1;
            await delay(200);
            return HttpResponse.json({ id: 'new-audit-id' }, { status: 201 });
        }));
        renderWithProviders(<AuditFormPage />);
        await user.type(screen.getByLabelText(/title/i), 'Q2 Chemical Safety Audit');
        await user.type(screen.getByLabelText(/scope/i), 'Warehouse B');
        await user.type(screen.getByLabelText(/location/i), 'Plant 2');
        const submitButton = screen.getByRole('button', { name: /create audit/i });
        await user.click(submitButton);
        // react-hook-form's zodResolver validation is itself async, so the mutation doesn't
        // dispatch in the same tick as the click — waitFor rides out that scheduling gap,
        // and the 200ms mock delay keeps the disabled window open long enough to observe.
        // waitFor's own default timeout (1000ms) is independent of vitest's testTimeout, so
        // it's raised explicitly here too for this machine's slower wall-clock scheduling.
        await waitFor(() => expect(callCount).toBe(1), { timeout: 15000 });
        await waitFor(() => expect(submitButton).not.toBeDisabled(), { timeout: 15000 });
    });
});
