import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/render';
import { FindingLifecycleActions } from '@/features/findings/components/FindingLifecycleActions';
const mockUseAuth = vi.fn();
vi.mock('@/auth/useAuth', () => ({ useAuth: () => mockUseAuth() }));
function baseFinding(status) {
    return {
        id: 'f1',
        auditId: 'a1',
        title: 'Finding',
        description: null,
        severity: 'High',
        status,
        rootCauseAnalysis: null,
        rootCauseAnalysisBy: null,
        rootCauseAnalysisAtUtc: null,
        createdBy: 'x',
        createdDate: '2026-08-01T00:00:00Z',
        modifiedBy: null,
        modifiedDate: null,
        rowVersion: 'AAAA',
        comments: [],
        documents: [],
    };
}
describe('FindingLifecycleActions', () => {
    it('offers only Start review for an Open finding', () => {
        mockUseAuth.mockReturnValue({ roles: ['Finding.Manage'] });
        renderWithProviders(<FindingLifecycleActions finding={baseFinding('Open')}/>);
        expect(screen.getByRole('button', { name: 'Start review' })).toBeInTheDocument();
        expect(screen.queryByRole('button', { name: 'Resolve' })).not.toBeInTheDocument();
    });
    it('offers no action once the finding is Closed', () => {
        mockUseAuth.mockReturnValue({ roles: ['Finding.Manage'] });
        renderWithProviders(<FindingLifecycleActions finding={baseFinding('Closed')}/>);
        expect(screen.queryByRole('button')).not.toBeInTheDocument();
    });
    it('hides actions when the caller lacks CanManageFindings', () => {
        mockUseAuth.mockReturnValue({ roles: [] });
        renderWithProviders(<FindingLifecycleActions finding={baseFinding('Open')}/>);
        expect(screen.queryByRole('button')).not.toBeInTheDocument();
    });
});
