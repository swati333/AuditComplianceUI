namespace Audit.Contracts.Requests;

/// <summary>
/// <see cref="RowVersion"/> is the optimistic-concurrency token the client
/// last read; a mismatch at save time produces HTTP 409 (ConflictException).
/// </summary>
public sealed record UpdateAuditRequest(string Title, string? Description, string Scope, string Location, byte[] RowVersion);
