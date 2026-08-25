import { lazy, Suspense, type ComponentType, type LazyExoticComponent } from 'react';
import { createBrowserRouter } from 'react-router';
import { AppShell } from '@/components/layout/AppShell';
import { LoadingState } from '@/components/feedback/LoadingState';
import { NotFoundPage } from '@/components/layout/NotFoundPage';
import { LoginPage } from '@/auth/LoginPage';
import { UnauthorizedPage } from '@/auth/UnauthorizedPage';
import { ProtectedRoute } from '@/auth/ProtectedRoute';
import type { PolicyName } from '@/config/policies';

const DashboardPage = lazy(() =>
  import('@/features/dashboard/pages/DashboardPage').then((m) => ({ default: m.DashboardPage })),
);
const AuditsListPage = lazy(() =>
  import('@/features/audits/pages/AuditsListPage').then((m) => ({ default: m.AuditsListPage })),
);
const AuditDetailPage = lazy(() =>
  import('@/features/audits/pages/AuditDetailPage').then((m) => ({ default: m.AuditDetailPage })),
);
const AuditFormPage = lazy(() =>
  import('@/features/audits/pages/AuditFormPage').then((m) => ({ default: m.AuditFormPage })),
);
const AuditChecklistPage = lazy(() =>
  import('@/features/audits/pages/AuditChecklistPage').then((m) => ({
    default: m.AuditChecklistPage,
  })),
);
const AuditHistoryPage = lazy(() =>
  import('@/features/audits/pages/AuditHistoryPage').then((m) => ({ default: m.AuditHistoryPage })),
);
const FindingsListPage = lazy(() =>
  import('@/features/findings/pages/FindingsListPage').then((m) => ({
    default: m.FindingsListPage,
  })),
);
const FindingDetailPage = lazy(() =>
  import('@/features/findings/pages/FindingDetailPage').then((m) => ({
    default: m.FindingDetailPage,
  })),
);
const FindingFormPage = lazy(() =>
  import('@/features/findings/pages/FindingFormPage').then((m) => ({ default: m.FindingFormPage })),
);
const FindingHistoryPage = lazy(() =>
  import('@/features/findings/pages/FindingHistoryPage').then((m) => ({
    default: m.FindingHistoryPage,
  })),
);
const ActionPlansPage = lazy(() =>
  import('@/features/action-plans/pages/ActionPlansPage').then((m) => ({
    default: m.ActionPlansPage,
  })),
);
const NotificationsListPage = lazy(() =>
  import('@/features/notifications/pages/NotificationsListPage').then((m) => ({
    default: m.NotificationsListPage,
  })),
);
const NotificationPreferencesPage = lazy(() =>
  import('@/features/notifications/pages/NotificationPreferencesPage').then((m) => ({
    default: m.NotificationPreferencesPage,
  })),
);
const ReportsPage = lazy(() =>
  import('@/features/reports/pages/ReportsPage').then((m) => ({ default: m.ReportsPage })),
);

function page(Component: LazyExoticComponent<ComponentType>, policy?: PolicyName) {
  return (
    <ProtectedRoute policy={policy}>
      <Suspense fallback={<LoadingState />}>
        <Component />
      </Suspense>
    </ProtectedRoute>
  );
}

export const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <AppShell />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: page(DashboardPage) },
      {
        path: 'unauthorized',
        element: (
          <ProtectedRoute>
            <UnauthorizedPage />
          </ProtectedRoute>
        ),
      },
      { path: 'audits', element: page(AuditsListPage) },
      { path: 'audits/new', element: page(AuditFormPage, 'CanManageAudits') },
      { path: 'audits/:auditId', element: page(AuditDetailPage) },
      { path: 'audits/:auditId/edit', element: page(AuditFormPage, 'CanManageAudits') },
      { path: 'audits/:auditId/checklist', element: page(AuditChecklistPage) },
      { path: 'audits/:auditId/history', element: page(AuditHistoryPage) },
      { path: 'audits/:auditId/findings', element: page(FindingsListPage) },
      { path: 'audits/:auditId/findings/new', element: page(FindingFormPage, 'CanManageFindings') },
      { path: 'findings', element: page(FindingsListPage) },
      { path: 'findings/new', element: page(FindingFormPage, 'CanManageFindings') },
      { path: 'findings/:findingId', element: page(FindingDetailPage) },
      { path: 'findings/:findingId/edit', element: page(FindingFormPage, 'CanManageFindings') },
      { path: 'findings/:findingId/history', element: page(FindingHistoryPage) },
      { path: 'action-plans', element: page(ActionPlansPage) },
      { path: 'notifications', element: page(NotificationsListPage) },
      { path: 'notifications/preferences', element: page(NotificationPreferencesPage) },
      { path: 'reports', element: page(ReportsPage, 'CanViewReports') },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
]);
