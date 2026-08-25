namespace Audit.Contracts.Dtos;

public sealed record AuditDetailDto(
    Guid Id,
    string Title,
    string? Description,
    string Scope,
    string Location,
    string Status,
    Guid? ChecklistId,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    DateTime? ActualStartDate,
    DateTime? ActualEndDate,
    string? CancellationReason,
    string CreatedBy,
    DateTime CreatedDate,
    string? ModifiedBy,
    DateTime? ModifiedDate,
    byte[] RowVersion,
    IReadOnlyCollection<AuditTeamMemberDto> TeamMembers,
    IReadOnlyCollection<ChecklistResponseDto> ChecklistResponses);
