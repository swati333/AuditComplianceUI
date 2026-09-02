import { baseApi } from '@/services/baseApi';
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
        getAudits: builder.query({
            query: (params) => ({ service: 'audit', url: '/audits', params: { ...params } }),
            providesTags: (result) => result
                ? [
                    ...result.items.map((a) => ({ type: 'Audit', id: a.id })),
                    { type: 'AuditList' },
                ]
                : [{ type: 'AuditList' }],
        }),
        getAuditById: builder.query({
            query: (id) => ({ service: 'audit', url: `/audits/${id}` }),
            providesTags: (_result, _error, id) => [{ type: 'Audit', id }],
        }),
        getAuditStatusHistory: builder.query({
            query: (id) => ({ service: 'audit', url: `/audits/${id}/status-history` }),
            providesTags: (_result, _error, id) => [{ type: 'Audit', id: `${id}-history` }],
        }),
        createAudit: builder.mutation({
            query: (body) => ({ service: 'audit', url: '/audits', method: 'POST', body }),
            invalidatesTags: ['AuditList'],
        }),
        updateAudit: builder.mutation({
            query: ({ id, body }) => ({ service: 'audit', url: `/audits/${id}`, method: 'PUT', body }),
            invalidatesTags: (_result, _error, { id }) => [{ type: 'Audit', id }, { type: 'AuditList' }],
        }),
        deleteAudit: builder.mutation({
            query: (id) => ({ service: 'audit', url: `/audits/${id}`, method: 'DELETE' }),
            invalidatesTags: ['AuditList'],
        }),
        assignTeamMember: builder.mutation({
            query: ({ auditId, body }) => ({
                service: 'audit',
                url: `/audits/${auditId}/team-members`,
                method: 'POST',
                body,
            }),
            invalidatesTags: (_result, _error, { auditId }) => [{ type: 'Audit', id: auditId }],
        }),
        removeTeamMember: builder.mutation({
            query: ({ auditId, teamMemberId }) => ({
                service: 'audit',
                url: `/audits/${auditId}/team-members/${teamMemberId}`,
                method: 'DELETE',
            }),
            async onQueryStarted({ auditId, teamMemberId }, { dispatch, queryFulfilled }) {
                const patch = dispatch(auditsApi.util.updateQueryData('getAuditById', auditId, (draft) => {
                    draft.teamMembers = draft.teamMembers.filter((m) => m.id !== teamMemberId);
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, { auditId }) => [{ type: 'Audit', id: auditId }],
        }),
        getChecklists: builder.query({
            query: () => ({ service: 'audit', url: '/checklists' }),
            providesTags: ['ChecklistList'],
        }),
        getChecklistById: builder.query({
            query: (id) => ({ service: 'audit', url: `/checklists/${id}` }),
            providesTags: (_result, _error, id) => [{ type: 'Checklist', id }],
        }),
        assignChecklist: builder.mutation({
            query: ({ auditId, body }) => ({
                service: 'audit',
                url: `/audits/${auditId}/checklist`,
                method: 'POST',
                body,
            }),
            invalidatesTags: (_result, _error, { auditId }) => [{ type: 'Audit', id: auditId }],
        }),
        recordChecklistResponse: builder.mutation({
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
                const patch = dispatch(auditsApi.util.updateQueryData('getAuditById', auditId, (draft) => {
                    const existing = draft.checklistResponses.find((r) => r.checklistQuestionId === body.checklistQuestionId);
                    if (existing) {
                        existing.answerText = body.answerText;
                        existing.isCompliant = body.isCompliant;
                    }
                    else {
                        draft.checklistResponses.push({
                            id: `optimistic-${body.checklistQuestionId}`,
                            checklistQuestionId: body.checklistQuestionId,
                            answerText: body.answerText,
                            isCompliant: body.isCompliant,
                            modifiedDate: new Date().toISOString(),
                        });
                    }
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, { auditId }) => [{ type: 'Audit', id: auditId }],
        }),
        planAudit: builder.mutation({
            query: ({ id, body }) => ({
                service: 'audit',
                url: `/audits/${id}/plan`,
                method: 'POST',
                body,
            }),
            async onQueryStarted({ id, body }, { dispatch, queryFulfilled }) {
                const patch = dispatch(auditsApi.util.updateQueryData('getAuditById', id, (draft) => {
                    draft.status = 'Planned';
                    draft.plannedStartDate = body.plannedStartDate;
                    draft.plannedEndDate = body.plannedEndDate;
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, { id }) => [{ type: 'Audit', id }, { type: 'AuditList' }],
        }),
        startAudit: builder.mutation({
            query: (id) => ({ service: 'audit', url: `/audits/${id}/start`, method: 'POST' }),
            async onQueryStarted(id, { dispatch, queryFulfilled }) {
                const patch = dispatch(auditsApi.util.updateQueryData('getAuditById', id, (draft) => {
                    draft.status = 'InProgress';
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, id) => [{ type: 'Audit', id }, { type: 'AuditList' }],
        }),
        completeAudit: builder.mutation({
            query: (id) => ({ service: 'audit', url: `/audits/${id}/complete`, method: 'POST' }),
            async onQueryStarted(id, { dispatch, queryFulfilled }) {
                const patch = dispatch(auditsApi.util.updateQueryData('getAuditById', id, (draft) => {
                    draft.status = 'Completed';
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, id) => [{ type: 'Audit', id }, { type: 'AuditList' }],
        }),
        closeAudit: builder.mutation({
            query: (id) => ({ service: 'audit', url: `/audits/${id}/close`, method: 'POST' }),
            async onQueryStarted(id, { dispatch, queryFulfilled }) {
                const patch = dispatch(auditsApi.util.updateQueryData('getAuditById', id, (draft) => {
                    draft.status = 'Closed';
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, id) => [{ type: 'Audit', id }, { type: 'AuditList' }],
        }),
        cancelAudit: builder.mutation({
            query: ({ id, body }) => ({
                service: 'audit',
                url: `/audits/${id}/cancel`,
                method: 'POST',
                body,
            }),
            async onQueryStarted({ id, body }, { dispatch, queryFulfilled }) {
                const patch = dispatch(auditsApi.util.updateQueryData('getAuditById', id, (draft) => {
                    draft.status = 'Cancelled';
                    draft.cancellationReason = body.reason ?? null;
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, { id }) => [{ type: 'Audit', id }, { type: 'AuditList' }],
        }),
    }),
});
export const { useGetAuditsQuery, useGetAuditByIdQuery, useGetAuditStatusHistoryQuery, useCreateAuditMutation, useUpdateAuditMutation, useDeleteAuditMutation, useAssignTeamMemberMutation, useRemoveTeamMemberMutation, useGetChecklistsQuery, useGetChecklistByIdQuery, useAssignChecklistMutation, useRecordChecklistResponseMutation, usePlanAuditMutation, useStartAuditMutation, useCompleteAuditMutation, useCloseAuditMutation, useCancelAuditMutation, } = auditsApi;
