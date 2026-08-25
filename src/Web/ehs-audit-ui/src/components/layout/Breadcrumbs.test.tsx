import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/render';
import { Breadcrumbs } from '@/components/layout/Breadcrumbs';

describe('Breadcrumbs', () => {
  it('renders nothing at the dashboard root', () => {
    const { container } = renderWithProviders(<Breadcrumbs />, { initialEntries: ['/'] });

    expect(container).toBeEmptyDOMElement();
  });

  it('renders a Dashboard > Audits trail for the audits list', () => {
    renderWithProviders(<Breadcrumbs />, { initialEntries: ['/audits'] });

    expect(screen.getByText('Dashboard')).toBeInTheDocument();
    expect(screen.getByText('Audits')).toBeInTheDocument();
  });

  it('labels a GUID segment as Details and keeps earlier segments as links', () => {
    renderWithProviders(<Breadcrumbs />, {
      initialEntries: ['/audits/11111111-1111-1111-1111-111111111111'],
    });

    expect(screen.getByRole('link', { name: 'Audits' })).toBeInTheDocument();
    expect(screen.getByText('Details')).toBeInTheDocument();
  });

  it('labels the new-audit route', () => {
    renderWithProviders(<Breadcrumbs />, { initialEntries: ['/audits/new'] });

    expect(screen.getByText('New')).toBeInTheDocument();
  });
});
