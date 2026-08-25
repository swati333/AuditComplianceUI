namespace ActionPlan.Contracts.Requests;

public sealed record AddActionPlanCommentRequest(string AuthorId, string AuthorName, string Text);
