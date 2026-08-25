namespace Finding.Contracts.Requests;

public sealed record UpdateFindingRequest(string Title, string? Description, string Severity, byte[] RowVersion);
