namespace Finding.Contracts.Requests;

public sealed record CreateFindingRequest(Guid AuditId, string Title, string? Description, string Severity);
