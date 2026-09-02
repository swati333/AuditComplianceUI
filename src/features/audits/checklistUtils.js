export function answerStateToIsCompliant(state) {
    if (state === 'compliant')
        return true;
    if (state === 'nonCompliant')
        return false;
    return null;
}
export function isCompliantToAnswerState(isCompliant) {
    if (isCompliant === true)
        return 'compliant';
    if (isCompliant === false)
        return 'nonCompliant';
    return 'notApplicable';
}
export function findResponse(responses, questionId) {
    return responses.find((r) => r.checklistQuestionId === questionId);
}
