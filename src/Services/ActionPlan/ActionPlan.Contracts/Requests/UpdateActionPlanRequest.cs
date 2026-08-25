namespace ActionPlan.Contracts.Requests;

public sealed record UpdateActionPlanRequest(
    string ActionType,
    string Title,
    string? Description,
    string OwnerId,
    string OwnerName,
    string ApproverId,
    string ApproverName,
    DateTime DueDate,
    string Priority,
    byte[] RowVersion);
