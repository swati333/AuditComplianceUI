namespace Audit.Contracts.Dtos;

public sealed record ChecklistResponseDto(Guid Id, Guid ChecklistQuestionId, string AnswerText, bool? IsCompliant, DateTime? ModifiedDate);
