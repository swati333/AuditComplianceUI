import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/render';
import { AuditLifecycleActions } from '@/features/audits/components/AuditLifecycleActions';
const mockUseAuth = vi.fn();
vi.mock('@/auth/useAuth', () => ({ useAuth: () => mockUseAuth() }));
function baseAudit(status) {
    return {
        id: 'a1',
        title: 'Audit',
        description: null,
        scope: 'Scope',
        location: 'Plant 1',
        status,
        checklistId: null,
        plannedStartDate: null,
        plannedEndDate: null,
        actualStartDate: null,
        actualEndDate: null,
        cancellationReason: null,
        createdBy: 'x',
        createdDate: '2026-08-01T00:00:00Z',
        modifiedBy: null,
        modifiedDate: null,
        rowVersion: 'AAAA',
        teamMembers: [],
        checklistResponses: [],
    };
}
describe('AuditLifecycleActions', () => {
    it('offers only Plan and Cancel for a Draft audit', () => {
        mockUseAuth.mockReturnValue({ roles: ['Audit.Manage', 'Audit.Perform'] });
        renderWithProviders(<AuditLifecycleActions audit={baseAudit('Draft')}/>);
        expect(screen.getByRole('button', { name: 'Plan audit' })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Cancel audit' })).toBeInTheDocument();
        expect(screen.queryByRole('button', { name: 'Start audit' })).not.toBeInTheDocument();
    });
    it('offers no Cancel action once the audit is Closed', () => {
        mockUseAuth.mockReturnValue({ roles: ['Audit.Manage', 'Audit.Perform'] });
        renderWithProviders(<AuditLifecycleActions audit={baseAudit('Closed')}/>);
        expect(screen.queryByRole('button', { name: 'Cancel audit' })).not.toBeInTheDocument();
        expect(screen.queryByRole('button', { name: 'Close audit' })).not.toBeInTheDocument();
    });
    it('hides all actions when the caller lacks both policies', () => {
        mockUseAuth.mockReturnValue({ roles: [] });
        renderWithProviders(<AuditLifecycleActions audit={baseAudit('Planned')}/>);
        expect(screen.queryByRole('button')).not.toBeInTheDocument();
    });
});
