import { z } from 'zod';

/**
 * Mirrors Finding.Application.Validators.CreateFindingRequestValidator /
 * UpdateFindingRequestValidator field-for-field (max lengths included).
 */
export const findingFormSchema = z.object({
  auditId: z.uuid('Select a valid audit'),
  title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or fewer'),
  description: z.string().max(4000, 'Description must be 4000 characters or fewer').optional(),
  severity: z.enum(['Low', 'Medium', 'High', 'Critical']),
});

export type FindingFormValues = z.infer<typeof findingFormSchema>;

/** Mirrors RecordRootCauseAnalysisRequestValidator (Text: required, max 8000). */
export const rootCauseAnalysisSchema = z.object({
  text: z
    .string()
    .min(1, 'Root cause analysis is required')
    .max(8000, 'Must be 8000 characters or fewer'),
});

export type RootCauseAnalysisFormValues = z.infer<typeof rootCauseAnalysisSchema>;

/** Mirrors AddCommentRequestValidator (Text: required, max 4000). */
export const commentSchema = z.object({
  text: z
    .string()
    .min(1, 'Comment cannot be empty')
    .max(4000, 'Comment must be 4000 characters or fewer'),
});

export type CommentFormValues = z.infer<typeof commentSchema>;

/**
 * Mirrors AddDocumentRequestValidator. BlobReference is a manual field here
 * because there's no blob-upload/SAS endpoint yet (CLAUDE.md §10) — this
 * records metadata for a file assumed already stored elsewhere, matching
 * exactly what AddDocumentRequest models today.
 */
export const addDocumentSchema = z.object({
  fileName: z.string().min(1, 'File name is required').max(260, 'Must be 260 characters or fewer'),
  blobReference: z
    .string()
    .min(1, 'Blob reference is required')
    .max(1000, 'Must be 1000 characters or fewer'),
  contentType: z
    .string()
    .min(1, 'Content type is required')
    .max(200, 'Must be 200 characters or fewer'),
  sizeBytes: z
    .number()
    .positive('Size must be greater than 0')
    .max(100 * 1024 * 1024, 'File must be 100 MB or smaller'),
});

export type AddDocumentFormValues = z.infer<typeof addDocumentSchema>;
