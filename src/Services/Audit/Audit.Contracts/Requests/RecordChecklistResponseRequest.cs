namespace Audit.Contracts.Requests;

public sealed record RecordChecklistResponseRequest(Guid ChecklistQuestionId, string AnswerText, bool? IsCompliant);
