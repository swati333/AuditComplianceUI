namespace Audit.Contracts.Requests;

public sealed record CreateAuditRequest(string Title, string? Description, string Scope, string Location);
