namespace ActionPlan.Contracts.Dtos;

public sealed record ActionPlanSummaryDto(
    Guid Id,
    Guid FindingId,
    string ActionType,
    string Title,
    string OwnerId,
    string OwnerName,
    string ApproverId,
    string ApproverName,
    DateTime DueDate,
    string Priority,
    string Status,
    DateTime CreatedDate,
    DateTime? ModifiedDate);
