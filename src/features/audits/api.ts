import { baseApi } from '@/services/baseApi';
import type { PagedResult } from '@/types/pagination';
import type {
  AssignChecklistRequest,
  AssignTeamMemberRequest,
  AuditDetail,
  AuditListQuery,
  AuditStatusHistoryEntry,
  AuditSummary,
  AuditTeamMember,
  CancelAuditRequest,
  Checklist,
  ChecklistResponse,
  CreateAuditRequest,
  PlanAuditRequest,
  RecordChecklistResponseRequest,
  UpdateAuditRequest,
} from '@/features/audits/types';

/**
 * Injected endpoints for Audit.Api's AuditsController and ChecklistsController
 * — routes/verbs match them 1:1 (CLAUDE.md §6). Lifecycle-transition mutations
 * (plan/start/complete/close/cancel) and team removal apply an optimistic
 * patch to the cached `getAuditById` entry: each one only changes a field the
 * client already knows the new value of (status, or a member being removed),
 * so rolling back to the previous cache snapshot on failure is always safe.
 * create/update/assignTeamMember are NOT optimistic — the server computes
 * fields (id, RowVersion, the new team member's id) the client can't predict.
 */
export const auditsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getAudits: builder.query<PagedResult<AuditSummary>, AuditListQuery>({
      query: (params) => ({ service: 'audit', url: '/audits', params: { ...params } }),
      providesTags: (result) =>
        result
          ? [
              ...result.items.map((a) => ({ type: 'Audit' as const, id: a.id })),
              { type: 'AuditList' as const },
            ]
          : [{ type: 'AuditList' as const }],
    }),

    getAuditById: builder.query<AuditDetail, string>({
      query: (id) => ({ service: 'audit', url: `/audits/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Audit', id }],
    }),

    getAuditStatusHistory: builder.query<AuditStatusHistoryEntry[], string>({
      query: (id) => ({ service: 'audit', url: `/audits/${id}/status-history` }),
      providesTags: (_result, _error, id) => [{ type: 'Audit', id: `${id}-history` }],
    }),

    createAudit: builder.mutation<AuditDetail, CreateAuditRequest>({
      query: (body) => ({ service: 'audit', url: '/audits', method: 'POST', body }),
      invalidatesTags: ['AuditList'],
    }),

    updateAudit: builder.mutation<AuditDetail, { id: string; body: UpdateAuditRequest }>({
      query: ({ id, body }) => ({ service: 'audit', url: `/audits/${id}`, method: 'PUT', body }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Audit', id }, { type: 'AuditList' }],
    }),

    deleteAudit: builder.mutation<void, string>({
      query: (id) => ({ service: 'audit', url: `/audits/${id}`, method: 'DELETE' }),
      invalidatesTags: ['AuditList'],
    }),

    assignTeamMember: builder.mutation<
      AuditTeamMember,
      { auditId: string; body: AssignTeamMemberRequest }
    >({
      query: ({ auditId, body }) => ({
        service: 'audit',
        url: `/audits/${auditId}/team-members`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { auditId }) => [{ type: 'Audit', id: auditId }],
    }),

    removeTeamMember: builder.mutation<void, { auditId: string; teamMemberId: string }>({
      query: ({ auditId, teamMemberId }) => ({
        service: 'audit',
        url: `/audits/${auditId}/team-members/${teamMemberId}`,
        method: 'DELETE',
      }),
      async onQueryStarted({ auditId, teamMemberId }, { dispatch, queryFulfilled }) {
        const patch = dispatch(
          auditsApi.util.updateQueryData('getAuditById', auditId, (draft) => {
            draft.teamMembers = draft.teamMembers.filter((m) => m.id !== teamMemberId);
          }),
        );
        try {
          await queryFulfilled;
        } catch {
          patch.undo();
        }
      },
      invalidatesTags: (_result, _error, { auditId }) => [{ type: 'Audit', id: auditId }],
    }),

    getChecklists: builder.query<Checklist[], void>({
      query: () => ({ service: 'audit', url: '/checklists' }),
      providesTags: ['ChecklistList'],
    }),

    getChecklistById: builder.query<Checklist, string>({
      query: (id) => ({ service: 'audit', url: `/checklists/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Checklist', id }],
    }),

    assignChecklist: builder.mutation<
      AuditDetail,
      { auditId: string; body: AssignChecklistRequest }
    >({
      query: ({ auditId, body }) => ({
        service: 'audit',
        url: `/audits/${auditId}/checklist`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { auditId }) => [{ type: 'Audit', id: auditId }],
    }),

    recordChecklistResponse: builder.mutation<
      ChecklistResponse,
      { auditId: string; body: RecordChecklistResponseRequest }
    >({
      query: ({ auditId, body }) => ({
        service: 'audit',
        url: `/audits/${auditId}/checklist-responses`,
        method: 'POST',
        body,
      }),
      async onQueryStarted({ auditId, body }, { dispatch, queryFulfilled }) {
        // Optimistic: only replaces/adds the one response for this question —
        // the server echoes the same answer back, so a failed save can simply
        // drop this optimistic entry and leave the previous one in place.
        const patch = dispatch(
          auditsApi.util.updateQueryData('getAuditById', auditId, (draft) => {
            const existing = draft.checklistResponses.find(
              (r) => r.checklistQuestionId === body.checklistQuestionId,
            );
            if (existing) {
              existing.answerText = body.answerText;
              existing.isCompliant = body.isCompliant;
            } else {
              draft.checklistResponses.push({
                id: `optimistic-${body.checklistQuestionId}`,
                checklistQuestionId: body.checklistQuestionId,
                answerText: body.answerText,
                isCompliant: body.isCompliant,
                modifiedDate: new Date().toISOString(),
              });
            }
          }),
        );
        try {
          await queryFulfilled;
        } catch {
          patch.undo();
        }
      },
      invalidatesTags: (_result, _error, { auditId }) => [{ type: 'Audit', id: auditId }],
    }),

    planAudit: builder.mutation<AuditDetail, { id: string; body: PlanAuditRequest }>({
      query: ({ id, body }) => ({
        service: 'audit',
        url: `/audits/${id}/plan`,
        method: 'POST',
        body,
      }),
      async onQueryStarted({ id, body }, { dispatch, queryFulfilled }) {
        const patch = dispatch(
          auditsApi.util.updateQueryData('getAuditById', id, (draft) => {
            draft.status = 'Planned';
            draft.plannedStartDate = body.plannedStartDate;
            draft.plannedEndDate = body.plannedEndDate;
          }),
        );
        try {
          await queryFulfilled;
        } catch {
          patch.undo();
        }
      },
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Audit', id }, { type: 'AuditList' }],
    }),

    startAudit: builder.mutation<AuditDetail, string>({
      query: (id) => ({ service: 'audit', url: `/audits/${id}/start`, method: 'POST' }),
      async onQueryStarted(id, { dispatch, queryFulfilled }) {
        const patch = dispatch(
          auditsApi.util.updateQueryData('getAuditById', id, (draft) => {
            draft.status = 'InProgress';
          }),
        );
        try {
          await queryFulfilled;
        } catch {
          patch.undo();
        }
      },
      invalidatesTags: (_result, _error, id) => [{ type: 'Audit', id }, { type: 'AuditList' }],
    }),

    completeAudit: builder.mutation<AuditDetail, string>({
      query: (id) => ({ service: 'audit', url: `/audits/${id}/complete`, method: 'POST' }),
      async onQueryStarted(id, { dispatch, queryFulfilled }) {
        const patch = dispatch(
          auditsApi.util.updateQueryData('getAuditById', id, (draft) => {
            draft.status = 'Completed';
          }),
        );
        try {
          await queryFulfilled;
        } catch {
          patch.undo();
        }
      },
      invalidatesTags: (_result, _error, id) => [{ type: 'Audit', id }, { type: 'AuditList' }],
    }),

    closeAudit: builder.mutation<AuditDetail, string>({
      query: (id) => ({ service: 'audit', url: `/audits/${id}/close`, method: 'POST' }),
      async onQueryStarted(id, { dispatch, queryFulfilled }) {
        const patch = dispatch(
          auditsApi.util.updateQueryData('getAuditById', id, (draft) => {
            draft.status = 'Closed';
          }),
        );
        try {
          await queryFulfilled;
        } catch {
          patch.undo();
        }
      },
      invalidatesTags: (_result, _error, id) => [{ type: 'Audit', id }, { type: 'AuditList' }],
    }),

    cancelAudit: builder.mutation<AuditDetail, { id: string; body: CancelAuditRequest }>({
      query: ({ id, body }) => ({
        service: 'audit',
        url: `/audits/${id}/cancel`,
        method: 'POST',
        body,
      }),
      async onQueryStarted({ id, body }, { dispatch, queryFulfilled }) {
        const patch = dispatch(
          auditsApi.util.updateQueryData('getAuditById', id, (draft) => {
            draft.status = 'Cancelled';
            draft.cancellationReason = body.reason ?? null;
          }),
        );
        try {
          await queryFulfilled;
        } catch {
          patch.undo();
        }
      },
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Audit', id }, { type: 'AuditList' }],
    }),
  }),
});

export const {
  useGetAuditsQuery,
  useGetAuditByIdQuery,
  useGetAuditStatusHistoryQuery,
  useCreateAuditMutation,
  useUpdateAuditMutation,
  useDeleteAuditMutation,
  useAssignTeamMemberMutation,
  useRemoveTeamMemberMutation,
  useGetChecklistsQuery,
  useGetChecklistByIdQuery,
  useAssignChecklistMutation,
  useRecordChecklistResponseMutation,
  usePlanAuditMutation,
  useStartAuditMutation,
  useCompleteAuditMutation,
  useCloseAuditMutation,
  useCancelAuditMutation,
} = auditsApi;
