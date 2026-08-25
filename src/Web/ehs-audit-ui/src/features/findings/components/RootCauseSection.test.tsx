import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/server';
import { renderWithProviders } from '@/test/render';
import { RootCauseSection } from '@/features/findings/components/RootCauseSection';
import { serviceBaseUrls } from '@/config/env';
import type { FindingDetail } from '@/features/findings/types';

const mockUseAuth = vi.fn();
vi.mock('@/auth/useAuth', () => ({ useAuth: () => mockUseAuth() }));

function baseFinding(overrides: Partial<FindingDetail> = {}): FindingDetail {
  return {
    id: 'f1',
    auditId: 'a1',
    title: 'Finding',
    description: null,
    severity: 'High',
    status: 'UnderReview',
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
    ...overrides,
  };
}

describe('RootCauseSection', () => {
  it('shows a form to record root cause analysis when none exists yet', () => {
    mockUseAuth.mockReturnValue({ roles: ['Finding.Manage'] });
    renderWithProviders(<RootCauseSection finding={baseFinding()} />);

    expect(screen.getByLabelText(/root cause analysis/i)).toBeInTheDocument();
  });

  it('shows the recorded analysis read-only once it exists', () => {
    mockUseAuth.mockReturnValue({ roles: ['Finding.Manage'] });
    renderWithProviders(
      <RootCauseSection
        finding={baseFinding({
          rootCauseAnalysis: 'Fire door propped open by staff.',
          rootCauseAnalysisBy: 'auditor@example.com',
          rootCauseAnalysisAtUtc: '2026-08-03T00:00:00Z',
        })}
      />,
    );

    expect(screen.getByText('Fire door propped open by staff.')).toBeInTheDocument();
    expect(screen.queryByLabelText(/root cause analysis/i)).not.toBeInTheDocument();
  });

  it('hides the form from callers without CanManageFindings', () => {
    mockUseAuth.mockReturnValue({ roles: [] });
    renderWithProviders(<RootCauseSection finding={baseFinding()} />);

    expect(screen.queryByLabelText(/root cause analysis/i)).not.toBeInTheDocument();
  });

  it('surfaces backend validation errors via ProblemDetails', async () => {
    mockUseAuth.mockReturnValue({ roles: ['Finding.Manage'] });
    server.use(
      http.post(`${serviceBaseUrls.finding}/findings/f1/root-cause-analysis`, () =>
        HttpResponse.json(
          { title: 'Validation error', detail: 'Text is required.', errorCode: 'VALIDATION_ERROR' },
          { status: 400 },
        ),
      ),
    );

    const user = userEvent.setup();
    renderWithProviders(<RootCauseSection finding={baseFinding()} />);

    await user.type(screen.getByLabelText(/root cause analysis/i), 'x');
    await user.click(screen.getByRole('button', { name: /save analysis/i }));

    expect(await screen.findByText('Text is required.')).toBeInTheDocument();
  });
});
