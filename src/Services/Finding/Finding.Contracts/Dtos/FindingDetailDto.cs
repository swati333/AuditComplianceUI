namespace Finding.Contracts.Dtos;

public sealed record FindingDetailDto(
    Guid Id,
    Guid AuditId,
    string Title,
    string? Description,
    string Severity,
    string Status,
    string? RootCauseAnalysis,
    string? RootCauseAnalysisBy,
    DateTime? RootCauseAnalysisAtUtc,
    string CreatedBy,
    DateTime CreatedDate,
    string? ModifiedBy,
    DateTime? ModifiedDate,
    byte[] RowVersion,
    IReadOnlyCollection<FindingCommentDto> Comments,
    IReadOnlyCollection<FindingDocumentDto> Documents);
