using Finding.Contracts.Dtos;
using Finding.Domain.Entities;
using FindingEntity = Finding.Domain.Entities.Finding;

namespace Finding.Application.Mapping;

public static class FindingMappingExtensions
{
    public static FindingSummaryDto ToSummaryDto(this FindingEntity finding) =>
        new(
            finding.Id,
            finding.AuditId,
            finding.Title,
            finding.Severity.ToString(),
            finding.Status.ToString(),
            finding.CreatedDate,
            finding.ModifiedDate);

    public static FindingDetailDto ToDetailDto(this FindingEntity finding) =>
        new(
            finding.Id,
            finding.AuditId,
            finding.Title,
            finding.Description,
            finding.Severity.ToString(),
            finding.Status.ToString(),
            finding.RootCauseAnalysis,
            finding.RootCauseAnalysisBy,
            finding.RootCauseAnalysisAtUtc,
            finding.CreatedBy,
            finding.CreatedDate,
            finding.ModifiedBy,
            finding.ModifiedDate,
            finding.RowVersion,
            finding.Comments.OrderBy(c => c.CreatedDate).Select(c => c.ToDto()).ToList(),
            finding.Documents.OrderBy(d => d.CreatedDate).Select(d => d.ToDto()).ToList());

    public static FindingCommentDto ToDto(this FindingComment comment) =>
        new(comment.Id, comment.AuthorId, comment.AuthorName, comment.Text, comment.CreatedDate);

    public static FindingDocumentDto ToDto(this FindingDocument document) =>
        new(document.Id, document.FileName, document.BlobReference, document.ContentType, document.SizeBytes, document.CreatedDate);

    public static FindingStatusHistoryDto ToDto(this FindingStatusHistory history) =>
        new(history.Id, history.FromStatus?.ToString(), history.ToStatus.ToString(), history.ChangedBy, history.ChangedAtUtc);
}
