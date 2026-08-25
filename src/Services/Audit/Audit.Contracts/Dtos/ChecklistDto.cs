namespace Audit.Contracts.Dtos;

public sealed record ChecklistDto(Guid Id, string Name, string? Description, bool IsActive, IReadOnlyCollection<ChecklistQuestionDto> Questions);
