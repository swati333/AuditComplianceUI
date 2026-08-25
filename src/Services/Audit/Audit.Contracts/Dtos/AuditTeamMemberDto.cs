namespace Audit.Contracts.Dtos;

public sealed record AuditTeamMemberDto(Guid Id, string UserId, string DisplayName, string Role);
