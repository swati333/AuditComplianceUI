namespace Audit.Contracts.Requests;

/// <summary>Role must be "Auditor" or "Auditee" (case-insensitive).</summary>
public sealed record AssignTeamMemberRequest(string UserId, string DisplayName, string Role);
