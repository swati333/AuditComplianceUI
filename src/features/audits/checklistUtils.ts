import type { ChecklistResponse } from '@/features/audits/types';

/**
 * ChecklistResponseDto.IsCompliant is a nullable bool on the backend — there
 * is no separate "not applicable" field. This UI treats that null as N/A
 * once a response exists; a question with no response at all yet is
 * "unanswered", which is a purely client-side concept (the array is just
 * missing an entry for that question id).
 */
export type AnswerState = 'compliant' | 'nonCompliant' | 'notApplicable';

export function answerStateToIsCompliant(state: AnswerState | undefined): boolean | null {
  if (state === 'compliant') return true;
  if (state === 'nonCompliant') return false;
  return null;
}

export function isCompliantToAnswerState(isCompliant: boolean | null): AnswerState {
  if (isCompliant === true) return 'compliant';
  if (isCompliant === false) return 'nonCompliant';
  return 'notApplicable';
}

export function findResponse(
  responses: ChecklistResponse[],
  questionId: string,
): ChecklistResponse | undefined {
  return responses.find((r) => r.checklistQuestionId === questionId);
}
