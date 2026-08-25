namespace Audit.Contracts.Dtos;

public sealed record ChecklistQuestionDto(Guid Id, string Text, bool IsMandatory, int DisplayOrder);
