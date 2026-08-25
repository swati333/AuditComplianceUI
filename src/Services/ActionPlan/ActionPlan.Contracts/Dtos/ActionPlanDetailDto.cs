namespace ActionPlan.Contracts.Dtos;

public sealed record ActionPlanDetailDto(
    Guid Id,
    Guid FindingId,
    string ActionType,
    string Title,
    string? Description,
    string OwnerId,
    string OwnerName,
    string ApproverId,
    string ApproverName,
    DateTime DueDate,
    string Priority,
    string Status,
    string? RejectionReason,
    string CreatedBy,
    DateTime CreatedDate,
    string? ModifiedBy,
    DateTime? ModifiedDate,
    byte[] RowVersion,
    IReadOnlyCollection<ActionPlanCommentDto> Comments,
    IReadOnlyCollection<ActionPlanEvidenceDto> Evidence);
