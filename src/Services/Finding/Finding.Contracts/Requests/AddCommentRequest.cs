namespace Finding.Contracts.Requests;

public sealed record AddCommentRequest(string AuthorId, string AuthorName, string Text);
