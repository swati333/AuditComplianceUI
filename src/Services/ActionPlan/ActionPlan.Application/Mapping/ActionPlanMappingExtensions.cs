using ActionPlan.Contracts.Dtos;
using ActionPlan.Domain.Entities;
using ActionPlanEntity = ActionPlan.Domain.Entities.ActionPlan;

namespace ActionPlan.Application.Mapping;

public static class ActionPlanMappingExtensions
{
    public static ActionPlanSummaryDto ToSummaryDto(this ActionPlanEntity actionPlan) =>
        new(
            actionPlan.Id,
            actionPlan.FindingId,
            actionPlan.ActionType.ToString(),
            actionPlan.Title,
            actionPlan.OwnerId,
            actionPlan.OwnerName,
            actionPlan.ApproverId,
            actionPlan.ApproverName,
            actionPlan.DueDate,
            actionPlan.Priority.ToString(),
            actionPlan.Status.ToString(),
            actionPlan.CreatedDate,
            actionPlan.ModifiedDate);

    public static ActionPlanDetailDto ToDetailDto(this ActionPlanEntity actionPlan) =>
        new(
            actionPlan.Id,
            actionPlan.FindingId,
            actionPlan.ActionType.ToString(),
            actionPlan.Title,
            actionPlan.Description,
            actionPlan.OwnerId,
            actionPlan.OwnerName,
            actionPlan.ApproverId,
            actionPlan.ApproverName,
            actionPlan.DueDate,
            actionPlan.Priority.ToString(),
            actionPlan.Status.ToString(),
            actionPlan.RejectionReason,
            actionPlan.CreatedBy,
            actionPlan.CreatedDate,
            actionPlan.ModifiedBy,
            actionPlan.ModifiedDate,
            actionPlan.RowVersion,
            actionPlan.Comments.OrderBy(c => c.CreatedDate).Select(c => c.ToDto()).ToList(),
            actionPlan.Evidence.OrderBy(e => e.CreatedDate).Select(e => e.ToDto()).ToList());

    public static ActionPlanCommentDto ToDto(this ActionPlanComment comment) =>
        new(comment.Id, comment.AuthorId, comment.AuthorName, comment.Text, comment.CreatedDate);

    public static ActionPlanEvidenceDto ToDto(this ActionPlanEvidence evidence) =>
        new(evidence.Id, evidence.FileName, evidence.BlobReference, evidence.ContentType, evidence.SizeBytes, evidence.CreatedDate);

    public static ActionPlanStatusHistoryDto ToDto(this ActionPlanStatusHistory history) =>
        new(history.Id, history.FromStatus?.ToString(), history.ToStatus.ToString(), history.ChangedBy, history.ChangedAtUtc, history.Reason);
}
