namespace Finding.Contracts.Dtos;

public sealed record FindingCommentDto(Guid Id, string AuthorId, string AuthorName, string Text, DateTime CreatedDate);
