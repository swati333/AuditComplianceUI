namespace ActionPlan.Contracts.Dtos;

public sealed record ActionPlanCommentDto(Guid Id, string AuthorId, string AuthorName, string Text, DateTime CreatedDate);
