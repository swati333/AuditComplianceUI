import { baseApi } from '@/services/baseApi';
/**
 * Injected endpoints for Finding.Api's FindingsController — routes/verbs
 * match it 1:1 (CLAUDE.md §6). Lifecycle-transition mutations apply an
 * optimistic patch to the cached `getFindingById` entry (only the status
 * field, which the client already knows the new value of), rolling back to
 * the previous snapshot on failure — safe because nothing else about the
 * finding is guessed. create/update/addComment/addDocument are NOT
 * optimistic: the server assigns their ids/timestamps.
 */
export const findingsApi = baseApi.injectEndpoints({
    endpoints: (builder) => ({
        getFindings: builder.query({
            query: (params) => ({ service: 'finding', url: '/findings', params: { ...params } }),
            providesTags: (result) => result
                ? [
                    ...result.items.map((f) => ({ type: 'Finding', id: f.id })),
                    { type: 'FindingList' },
                ]
                : [{ type: 'FindingList' }],
        }),
        getFindingById: builder.query({
            query: (id) => ({ service: 'finding', url: `/findings/${id}` }),
            providesTags: (_result, _error, id) => [{ type: 'Finding', id }],
        }),
        getFindingStatusHistory: builder.query({
            query: (id) => ({ service: 'finding', url: `/findings/${id}/status-history` }),
            providesTags: (_result, _error, id) => [{ type: 'Finding', id: `${id}-history` }],
        }),
        createFinding: builder.mutation({
            query: (body) => ({ service: 'finding', url: '/findings', method: 'POST', body }),
            invalidatesTags: ['FindingList'],
        }),
        updateFinding: builder.mutation({
            query: ({ id, body }) => ({
                service: 'finding',
                url: `/findings/${id}`,
                method: 'PUT',
                body,
            }),
            invalidatesTags: (_result, _error, { id }) => [
                { type: 'Finding', id },
                { type: 'FindingList' },
            ],
        }),
        deleteFinding: builder.mutation({
            query: (id) => ({ service: 'finding', url: `/findings/${id}`, method: 'DELETE' }),
            invalidatesTags: ['FindingList'],
        }),
        recordRootCauseAnalysis: builder.mutation({
            query: ({ id, body }) => ({
                service: 'finding',
                url: `/findings/${id}/root-cause-analysis`,
                method: 'POST',
                body,
            }),
            invalidatesTags: (_result, _error, { id }) => [{ type: 'Finding', id }],
        }),
        addFindingComment: builder.mutation({
            query: ({ id, body }) => ({
                service: 'finding',
                url: `/findings/${id}/comments`,
                method: 'POST',
                body,
            }),
            invalidatesTags: (_result, _error, { id }) => [{ type: 'Finding', id }],
        }),
        addFindingDocument: builder.mutation({
            query: ({ id, body }) => ({
                service: 'finding',
                url: `/findings/${id}/documents`,
                method: 'POST',
                body,
            }),
            invalidatesTags: (_result, _error, { id }) => [{ type: 'Finding', id }],
        }),
        startFindingReview: builder.mutation({
            query: (id) => ({ service: 'finding', url: `/findings/${id}/start-review`, method: 'POST' }),
            async onQueryStarted(id, { dispatch, queryFulfilled }) {
                const patch = dispatch(findingsApi.util.updateQueryData('getFindingById', id, (draft) => {
                    draft.status = 'UnderReview';
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, id) => [{ type: 'Finding', id }, { type: 'FindingList' }],
        }),
        requireFindingAction: builder.mutation({
            query: (id) => ({
                service: 'finding',
                url: `/findings/${id}/require-action`,
                method: 'POST',
            }),
            async onQueryStarted(id, { dispatch, queryFulfilled }) {
                const patch = dispatch(findingsApi.util.updateQueryData('getFindingById', id, (draft) => {
                    draft.status = 'ActionRequired';
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, id) => [{ type: 'Finding', id }, { type: 'FindingList' }],
        }),
        resolveFinding: builder.mutation({
            query: (id) => ({ service: 'finding', url: `/findings/${id}/resolve`, method: 'POST' }),
            async onQueryStarted(id, { dispatch, queryFulfilled }) {
                const patch = dispatch(findingsApi.util.updateQueryData('getFindingById', id, (draft) => {
                    draft.status = 'Resolved';
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, id) => [{ type: 'Finding', id }, { type: 'FindingList' }],
        }),
        verifyFinding: builder.mutation({
            query: (id) => ({ service: 'finding', url: `/findings/${id}/verify`, method: 'POST' }),
            async onQueryStarted(id, { dispatch, queryFulfilled }) {
                const patch = dispatch(findingsApi.util.updateQueryData('getFindingById', id, (draft) => {
                    draft.status = 'Verified';
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, id) => [{ type: 'Finding', id }, { type: 'FindingList' }],
        }),
        closeFinding: builder.mutation({
            query: (id) => ({ service: 'finding', url: `/findings/${id}/close`, method: 'POST' }),
            async onQueryStarted(id, { dispatch, queryFulfilled }) {
                const patch = dispatch(findingsApi.util.updateQueryData('getFindingById', id, (draft) => {
                    draft.status = 'Closed';
                }));
                try {
                    await queryFulfilled;
                }
                catch {
                    patch.undo();
                }
            },
            invalidatesTags: (_result, _error, id) => [{ type: 'Finding', id }, { type: 'FindingList' }],
        }),
    }),
});
export const { useGetFindingsQuery, useGetFindingByIdQuery, useGetFindingStatusHistoryQuery, useCreateFindingMutation, useUpdateFindingMutation, useDeleteFindingMutation, useRecordRootCauseAnalysisMutation, useAddFindingCommentMutation, useAddFindingDocumentMutation, useStartFindingReviewMutation, useRequireFindingActionMutation, useResolveFindingMutation, useVerifyFindingMutation, useCloseFindingMutation, } = findingsApi;
