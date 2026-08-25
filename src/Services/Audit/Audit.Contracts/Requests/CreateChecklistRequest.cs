namespace Audit.Contracts.Requests;

public sealed record CreateChecklistRequest(string Name, string? Description, IReadOnlyCollection<CreateChecklistQuestionRequest> Questions);

public sealed record CreateChecklistQuestionRequest(string Text, bool IsMandatory, int DisplayOrder);
