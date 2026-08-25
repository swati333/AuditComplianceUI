using Audit.Contracts.Dtos;
using Audit.Domain.Entities;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.Application.Mapping;

/// <summary>
/// Domain → Contracts DTO mapping. Deliberately hand-written rather than an
/// automapper: the DTO shape is small and stable enough that reflection-based
/// mapping would add a dependency without saving meaningful code.
/// </summary>
public static class AuditMappingExtensions
{
    public static AuditSummaryDto ToSummaryDto(this AuditEntity audit) =>
        new(
            audit.Id,
            audit.Title,
            audit.Scope,
            audit.Location,
            audit.Status.ToString(),
            audit.PlannedStartDate,
            audit.PlannedEndDate,
            audit.CreatedDate,
            audit.ModifiedDate);

    public static AuditDetailDto ToDetailDto(this AuditEntity audit) =>
        new(
            audit.Id,
            audit.Title,
            audit.Description,
            audit.Scope,
            audit.Location,
            audit.Status.ToString(),
            audit.ChecklistId,
            audit.PlannedStartDate,
            audit.PlannedEndDate,
            audit.ActualStartDate,
            audit.ActualEndDate,
            audit.CancellationReason,
            audit.CreatedBy,
            audit.CreatedDate,
            audit.ModifiedBy,
            audit.ModifiedDate,
            audit.RowVersion,
            audit.TeamMembers.Select(m => m.ToDto()).ToList(),
            audit.ChecklistResponses.Select(r => r.ToDto()).ToList());

    public static AuditTeamMemberDto ToDto(this AuditTeamMember member) =>
        new(member.Id, member.UserId, member.DisplayName, member.Role.ToString());

    public static ChecklistResponseDto ToDto(this ChecklistResponse response) =>
        new(response.Id, response.ChecklistQuestionId, response.AnswerText, response.IsCompliant, response.ModifiedDate);

    public static AuditStatusHistoryDto ToDto(this AuditStatusHistory history) =>
        new(history.Id, history.FromStatus?.ToString(), history.ToStatus.ToString(), history.ChangedBy, history.ChangedAtUtc, history.Reason);

    public static ChecklistDto ToDto(this Checklist checklist) =>
        new(
            checklist.Id,
            checklist.Name,
            checklist.Description,
            checklist.IsActive,
            checklist.Questions.OrderBy(q => q.DisplayOrder).Select(q => q.ToDto()).ToList());

    public static ChecklistQuestionDto ToDto(this ChecklistQuestion question) =>
        new(question.Id, question.Text, question.IsMandatory, question.DisplayOrder);
}
