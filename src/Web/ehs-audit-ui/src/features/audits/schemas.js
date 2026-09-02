import { z } from 'zod';
/**
 * Mirrors Audit.Application.Validators.CreateAuditRequestValidator /
 * UpdateAuditRequestValidator field-for-field (max lengths included) —
 * client-side validation ahead of the API's FluentValidation (CLAUDE.md §9).
 */
export const auditFormSchema = z.object({
    title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or fewer'),
    description: z.string().max(2000, 'Description must be 2000 characters or fewer').optional(),
    scope: z.string().min(1, 'Scope is required').max(1000, 'Scope must be 1000 characters or fewer'),
    location: z
        .string()
        .min(1, 'Location is required')
        .max(300, 'Location must be 300 characters or fewer'),
});
export const planAuditSchema = z
    .object({
    plannedStartDate: z.string().min(1, 'Start date is required'),
    plannedEndDate: z.string().min(1, 'End date is required'),
})
    .refine((value) => new Date(value.plannedEndDate) >= new Date(value.plannedStartDate), {
    message: 'End date must be on or after the start date',
    path: ['plannedEndDate'],
});
export const assignTeamMemberSchema = z.object({
    userId: z
        .string()
        .min(1, 'User ID is required')
        .max(200, 'User ID must be 200 characters or fewer'),
    displayName: z
        .string()
        .min(1, 'Display name is required')
        .max(200, 'Display name must be 200 characters or fewer'),
    role: z.enum(['Auditor', 'Auditee']),
});
/** Mirrors Audit.Application.Validators.CancelAuditRequestValidator — Reason is optional, max 1000. */
export const cancelAuditSchema = z.object({
    reason: z.string().max(1000, 'Reason must be 1000 characters or fewer').optional(),
});
/** Mirrors Audit.Application.Validators.AssignChecklistRequestValidator. */
export const assignChecklistSchema = z.object({
    checklistId: z.uuid('Select a checklist'),
});
/**
 * Mirrors Audit.Application.Validators.RecordChecklistResponseRequestValidator
 * (AnswerText: required, max 4000) plus the client-only "answer state" this
 * UI adds on top: which of Compliant/NonCompliant/NotApplicable was chosen.
 * Mandatory questions must have both an answer state and non-empty text.
 */
export const checklistAnswerSchema = z
    .object({
    answerState: z.enum(['compliant', 'nonCompliant', 'notApplicable']).optional(),
    answerText: z.string().max(4000, 'Must be 4000 characters or fewer').optional(),
    isMandatory: z.boolean(),
})
    .refine((value) => !value.isMandatory || value.answerState !== undefined, {
    message: 'This question is required',
    path: ['answerState'],
})
    .refine((value) => !value.isMandatory || Boolean(value.answerText?.trim()), {
    message: 'A comment is required for this question',
    path: ['answerText'],
});
